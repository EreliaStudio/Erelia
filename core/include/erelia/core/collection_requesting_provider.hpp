#pragma once
#include "erelia/core/networking/collection_protocol.hpp"
#include <diagnostics/logger.hpp>
#include <limits>

template <typename TKey, typename TElement>
class Collection<TKey, TElement>::RequestingProvider : public Collection<TKey, TElement>::Provider
{
	using Base = typename Collection::Provider;
	using Protocol = Networking::CollectionProtocol<TKey, TElement>;
	using ID = spk::Message::RequestID;
	struct Parsing
	{
		typename Protocol::Response response;
		typename spk::TaskGroup<typename Protocol::Section>::Answer answer;
		typename spk::TaskGroup<typename Protocol::Section>::Answer::CompletionContract contract;
		std::atomic_bool finished{false};
		Parsing(typename Protocol::Response value, typename spk::TaskGroup<typename Protocol::Section>::Answer group) :
			response(std::move(value)),
			answer(std::move(group))
		{
		}
	};
	struct NetworkState
	{
		std::recursive_mutex sendMutex;
		ID nextID = 1;
		bool exhausted = false;
		std::unordered_map<ID, std::unordered_map<TKey, std::shared_ptr<Task>>> requests;
		std::unordered_map<TKey, typename Protocol::Failure> refused;
		std::vector<std::shared_ptr<Parsing>> parsing;
	};
	std::shared_ptr<NetworkState> _network = std::make_shared<NetworkState>();
	spk::WorkerPool &_pool;
	std::function<void(const spk::Message &)> _send;

protected:
	[[nodiscard]] Answer _acquire(const TKey &) override
	{
		throw spk::Exception("Network acquisition must be batched");
	}
	void _start(const std::vector<TKey> &keys) override
	{
		std::vector<std::pair<TKey, std::shared_ptr<Task>>> acquisitions;
		std::vector<typename Base::Completion> completed;
		{
			const std::scoped_lock lock(this->_state->mutex);
			for (const auto &key : keys)
			{
				auto pending = this->_state->pending.find(key);
				if (pending == this->_state->pending.end() || pending->second.started == true)
				{
					continue;
				}
				pending->second.started = true;
				auto refused = _network->refused.find(key);
				if (refused != _network->refused.end())
				{
					auto completion = Base::_claim(this->_state, key, pending->second.task, std::nullopt, std::make_exception_ptr(spk::Exception(refused->second.message)));
					if (completion.has_value() == true)
					{
						completed.push_back(std::move(*completion));
					}
				}
				else
				{
					acquisitions.emplace_back(key, pending->second.task);
				}
			}
		}
		for (std::size_t begin = 0; begin < acquisitions.size(); begin += TElement::MaximumElementsPerRequest)
		{
			const auto end = std::min(begin + TElement::MaximumElementsPerRequest, acquisitions.size());
			_emit({acquisitions.begin() + begin, acquisitions.begin() + end});
		}
		for (auto &completion : completed)
		{
			Base::_complete(std::move(completion));
		}
	}

private:
	void _emit(const std::vector<std::pair<TKey, std::shared_ptr<Task>>> &keys)
	{
		std::unordered_map<TKey, std::shared_ptr<Task>> acquisitions;
		std::vector<TKey> active;
		std::exception_ptr failure;
		{
			// Serialize ID assignment and emission without holding the acquisition mutex during send.
			const std::scoped_lock sending(_network->sendMutex);
			ID id = 0;
			{
				const std::scoped_lock lock(this->_state->mutex);
				for (const auto &[key, task] : keys)
				{
					auto pending = this->_state->pending.find(key);
					if (pending != this->_state->pending.end() && pending->second.task == task)
					{
						acquisitions.emplace(key, task);
						active.push_back(key);
					}
				}
				if (active.empty() == true)
				{
					return;
				}
				if (_network->exhausted == true)
				{
					failure = std::make_exception_ptr(spk::Exception("Collection RequestID exhausted"));
				}
				else
				{
					id = _network->nextID;
					if (id == std::numeric_limits<ID>::max())
					{
						_network->exhausted = true;
					}
					else
					{
						++_network->nextID;
					}
					_network->requests.emplace(id, acquisitions);
				}
			}
			if (failure == nullptr)
			{
				try
				{
					_send(Protocol::Request::build(id, active));
				} catch (...)
				{
					failure = std::current_exception();
					const std::scoped_lock lock(this->_state->mutex);
					_network->requests.erase(id);
				}
			}
		}
		if (failure != nullptr)
		{
			_fail(acquisitions, failure);
		}
	}
	void _fail(const std::unordered_map<TKey, std::shared_ptr<Task>> &tasks, std::exception_ptr failure)
	{
		for (const auto &[key, task] : tasks)
		{
			Base::_settle(this->_state, key, task, std::nullopt, failure);
		}
	}
	static void apply(const std::shared_ptr<typename Base::AcquisitionState> &state, const std::shared_ptr<NetworkState> &network, const Parsing &parsing)
	{
		std::vector<typename Base::Completion> completed;
		{
			const std::scoped_lock lock(state->mutex);
			auto found = network->requests.find(parsing.response.requestID());
			if (found == network->requests.end())
			{
				return;
			}
			if (parsing.answer.status() == spk::Task<typename Protocol::Section>::Status::Failed)
			{
				SPK_LOG(Warning) << "Malformed Collection Response section" << std::endl;
				return;
			}
			std::set<TKey> seen;
			std::optional<TKey> previous;
			for (std::size_t index = 0; index < parsing.answer.size(); ++index)
			{
				if (index == parsing.response.firstSectionCount())
				{
					previous.reset();
				}
				const auto &section = parsing.answer.at(index).result();
				std::vector<TKey> keys;
				for (const auto &value : section.success)
				{
					keys.push_back(value.key);
				}
				for (const auto &value : section.failure)
				{
					keys.push_back(value.key);
				}
				for (const auto &key : keys)
				{
					if (seen.insert(key).second == false || found->second.contains(key) == false || (previous.has_value() == true && (*previous < key) == false))
					{
						SPK_LOG(Warning) << "Malformed Collection Response entries" << std::endl;
						return;
					}
					previous = key;
				}
			}
			if (seen.size() != found->second.size())
			{
				SPK_LOG(Warning) << "Incomplete Collection Response" << std::endl;
				return;
			}
			const auto acquisitions = std::move(found->second);
			network->requests.erase(found);
			for (const auto &answer : parsing.answer.answers())
			{
				for (const auto &success : answer.result().success)
				{
					auto task = acquisitions.find(success.key);
					if (task != acquisitions.end())
					{
						auto completion = Base::_claim(state, success.key, task->second, success.element, nullptr);
						if (completion.has_value() == true)
						{
							completed.push_back(std::move(*completion));
						}
					}
				}
				for (const auto &failure : answer.result().failure)
				{
					auto task = acquisitions.find(failure.key);
					auto pending = state->pending.find(failure.key);
					if (task != acquisitions.end() && pending != state->pending.end() && pending->second.task == task->second)
					{
						network->refused.insert_or_assign(failure.key, failure.failure);
						auto completion = Base::_claim(state, failure.key, task->second, std::nullopt, std::make_exception_ptr(spk::Exception(failure.failure.message)));
						if (completion.has_value() == true)
						{
							completed.push_back(std::move(*completion));
						}
					}
				}
			}
		}
		for (auto &completion : completed)
		{
			Base::_complete(std::move(completion));
		}
	}

public:
	RequestingProvider(spk::WorkerPool &pool, std::function<void(const spk::Message &)> send) :
		_pool(pool),
		_send(std::move(send))
	{
	}
	void receive(const spk::Message &message)
	{
		typename Protocol::Response response(message);
		{
			const std::scoped_lock lock(this->_state->mutex);
			if (_network->requests.contains(response.requestID()) == false)
			{
				return;
			}
		}
		spk::TaskGroup<typename Protocol::Section> group;
		for (std::size_t index = 0; index < response.sectionCount(); ++index)
		{
			group.add(_pool.submit([response, index] {
				return response.section(index);
			}));
		}
		auto context = std::make_shared<Parsing>(std::move(response), std::move(group).answer());
		const std::weak_ptr<Parsing> weak = context;
		const std::weak_ptr<typename Base::AcquisitionState> state = this->_state;
		const std::weak_ptr<NetworkState> network = _network;
		std::vector<std::shared_ptr<Parsing>> retired;
		{
			const std::scoped_lock lock(this->_state->mutex);
			for (auto &entry : _network->parsing)
			{
				if (entry->finished.load() == true)
				{
					retired.push_back(std::move(entry));
				}
			}
			std::erase(_network->parsing, nullptr);
			_network->parsing.push_back(context);
		}
		context->contract = context->answer.subscribeToCompletion([weak, state, network] {
			auto current = weak.lock();
			auto storage = state.lock();
			auto remote = network.lock();
			if (current != nullptr && storage != nullptr && remote != nullptr)
			{
				apply(storage, remote, *current);
				current->finished.store(true);
			}
		});
	}
	void receiveError(const spk::Message &message)
	{
		const typename Protocol::Error error(message);
		SPK_LOG(Warning) << error.diagnostic().message << " (" << error.keys().size() << " contextual keys)" << std::endl;
	}
	void disconnect() override
	{
		std::vector<std::shared_ptr<Parsing>> parsing;
		{
			const std::scoped_lock lock(this->_state->mutex);
			_network->requests.clear();
			_network->refused.clear();
			parsing.swap(_network->parsing);
		}
		Base::disconnect();
	}
};
