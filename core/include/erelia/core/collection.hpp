#pragma once

#include <concepts>
#include <functional>
#include <memory>
#include <mutex>
#include <optional>
#include <unordered_map>
#include <utility>
#include <vector>

#include <container/protected_data.hpp>
#include <exception.hpp>
#include <network/message.hpp>
#include <threading/task_group.hpp>
#include <threading/worker_pool.hpp>

// Serialization is checked on the actual Writer/Reader API, including ADL.
template <typename T>
concept MessageSerializable = requires(spk::Message::Writer &writer, const spk::Message::Reader &reader, const T &input, T &output) {
	writer << input;
	reader >> output;
};

template <typename TKey, typename TElement>
class Collection
{
	static_assert(MessageSerializable<TKey> && MessageSerializable<TElement>);

public:
	enum class State
	{
		Absent,
		Pending,
		Available
	};
	using Task = spk::Task<TElement>;
	using Answer = typename Task::Answer;
	class Provider;
	class GeneratingProvider;
	class RequestingProvider;
	class Updater;

private:
	using Storage = spk::ProtectedData<std::unordered_map<TKey, TElement>>;
	std::shared_ptr<Storage> _storage = std::make_shared<Storage>();
	std::unique_ptr<Provider> _provider;

public:
	template <typename TProvider>
		requires std::derived_from<std::remove_cvref_t<TProvider>, Provider> && (std::is_lvalue_reference_v<TProvider> == false)
	explicit Collection(TProvider &&provider) :
		_provider(std::make_unique<std::remove_cvref_t<TProvider>>(std::forward<TProvider>(provider)))
	{
		_provider->_state->storage = _storage;
	}
	~Collection()
	{
		_provider->disconnect();
	}
	Collection(const Collection &) = delete;
	Collection &operator=(const Collection &) = delete;
	[[nodiscard]] Provider &provider() noexcept
	{
		return *_provider;
	}
	[[nodiscard]] State state(const TKey &key) const
	{
		const std::scoped_lock lock(_provider->_state->mutex);
		if (_storage->read()->contains(key) == true)
		{
			return State::Available;
		}
		return _provider->_state->pending.contains(key) == true ? State::Pending : State::Absent;
	}
	template <typename TCallback>
	bool tryRead(const TKey &key, TCallback &&callback) const
	{
		const auto reader = _storage->read();
		const auto found = reader->find(key);
		if (found == reader->end())
		{
			return false;
		}
		std::invoke(std::forward<TCallback>(callback), std::as_const(found->second));
		return true;
	}
	[[nodiscard]] Answer request(const TKey &key)
	{
		const auto group = request(std::vector<TKey>{key});
		return group.at(0);
	}
	[[nodiscard]] typename spk::TaskGroup<TElement>::Answer request(const std::vector<TKey> &keys)
	{
		spk::TaskGroup<TElement> group;
		std::vector<TKey> missing;
		{
			const std::scoped_lock lock(_provider->_state->mutex);
			for (const TKey &key : keys)
			{
				group.add(_provider->_request(key, missing));
			}
		}
		_provider->_start(missing);
		_provider->reclaim();
		return std::move(group).answer();
	}
	void insert(const TKey &key, TElement element)
	{
		_mutate(key, std::move(element), false);
	}
	void replace(const TKey &key, TElement element)
	{
		_mutate(key, std::move(element), true);
	}
	void remove(const TKey &key)
	{
		std::shared_ptr<Task> invalidated;
		{
			const std::scoped_lock lock(_provider->_state->mutex);
			invalidated = _provider->_invalidate(key);
			_storage->write()->erase(key);
		}
		Provider::_failInvalidated(invalidated);
		_provider->reclaim();
	}
	[[nodiscard]] std::vector<TKey> keys() const
	{
		const std::scoped_lock lock(_provider->_state->mutex);
		std::vector<TKey> result;
		{
			const auto reader = _storage->read();
			for (const auto &[key, value] : *reader)
			{
				result.push_back(key);
			}
		}
		for (const auto &[key, pending] : _provider->_state->pending)
		{
			result.push_back(key);
		}
		return result;
	}

private:
	void _mutate(const TKey &key, TElement element, bool replacing)
	{
		std::shared_ptr<Task> invalidated;
		std::exception_ptr failure;
		{
			const std::scoped_lock lock(_provider->_state->mutex);
			invalidated = _provider->_invalidate(key);
			try
			{
				auto writer = _storage->write();
				if (writer->contains(key) != replacing)
				{
					throw spk::Exception("Collection mutation requires the matching Available state");
				}
				writer->insert_or_assign(key, std::move(element));
			} catch (...)
			{
				failure = std::current_exception();
			}
		}
		Provider::_failInvalidated(invalidated);
		_provider->reclaim();
		if (failure != nullptr)
		{
			std::rethrow_exception(failure);
		}
	}
};

template <typename TKey, typename TElement>
class Collection<TKey, TElement>::Provider
{
	friend class Collection;
	friend class Updater;

protected:
	struct Pending
	{
		Answer answer;
		typename Answer::CompletionContract contract;
		std::shared_ptr<Task> task;
		bool started = false;
	};
	struct AcquisitionState
	{
		std::recursive_mutex mutex;
		std::unordered_map<TKey, Pending> pending;
		std::shared_ptr<Storage> storage;
		std::vector<typename Answer::CompletionContract> retired;
	};
	std::shared_ptr<AcquisitionState> _state = std::make_shared<AcquisitionState>();
	[[nodiscard]] virtual Answer _acquire(const TKey &key) = 0;
	virtual void _start(const std::vector<TKey> &keys)
	{
		for (const auto &key : keys)
		{
			std::shared_ptr<Task> task;
			{
				const std::scoped_lock lock(_state->mutex);
				auto found = _state->pending.find(key);
				if (found == _state->pending.end() || found->second.started == true)
				{
					continue;
				}
				found->second.started = true;
				task = found->second.task;
			}
			try
			{
				_observe(key, task, _acquire(key));
			} catch (...)
			{
				_settle(_state, key, task, std::nullopt, std::current_exception());
			}
		}
	}
	struct Completion
	{
		std::shared_ptr<Task> task;
		std::optional<TElement> element;
		std::exception_ptr failure;
	};
	static std::optional<Completion> _claim(const std::shared_ptr<AcquisitionState> &state, const TKey &key, std::shared_ptr<Task> task, std::optional<TElement> element, std::exception_ptr failure)
	{
		const std::scoped_lock lock(state->mutex);
		auto found = state->pending.find(key);
		if (found == state->pending.end() || found->second.task != task)
		{
			return std::nullopt;
		}
		if (element.has_value() == true)
		{
			state->storage->write()->insert_or_assign(key, *element);
		}
		state->retired.push_back(std::move(found->second.contract));
		state->pending.erase(found);
		return Completion{std::move(task), std::move(element), std::move(failure)};
	}
	static void _complete(Completion completion)
	{
		// Notify outside the acquisition mutex: subscribers may acquire or invalidate other keys.
		if (completion.element.has_value() == true)
		{
			completion.task->validate(std::move(*completion.element));
		}
		else
		{
			completion.task->fail(completion.failure);
		}
	}
	static bool _settle(const std::shared_ptr<AcquisitionState> &state, const TKey &key, std::shared_ptr<Task> task, std::optional<TElement> element, std::exception_ptr failure)
	{
		auto completion = _claim(state, key, std::move(task), std::move(element), std::move(failure));
		if (completion.has_value() == false)
		{
			return false;
		}
		_complete(std::move(*completion));
		return true;
	}
	void _observe(const TKey &key, const std::shared_ptr<Task> &task, Answer source)
	{
		const std::weak_ptr<AcquisitionState> weak = _state;
		auto contract = source.subscribeToCompletion([weak, key, task, source] {
			if (auto state = weak.lock(); state != nullptr)
			{
				if (source.status() == Task::Status::Completed)
				{
					_settle(state, key, task, source.result(), nullptr);
				}
				else
				{
					_settle(state, key, task, std::nullopt, source.failure());
				}
			}
		});
		const std::scoped_lock lock(_state->mutex);
		auto found = _state->pending.find(key);
		if (found != _state->pending.end() && found->second.task == task)
		{
			found->second.contract = std::move(contract);
		}
	}
	[[nodiscard]] std::shared_ptr<Task> _invalidate(const TKey &key)
	{
		auto found = _state->pending.find(key);
		if (found == _state->pending.end())
		{
			return nullptr;
		}
		auto task = found->second.task;
		_state->retired.push_back(std::move(found->second.contract));
		_state->pending.erase(found);
		return task;
	}
	static void _failInvalidated(const std::shared_ptr<Task> &task)
	{
		if (task != nullptr)
		{
			task->fail(std::make_exception_ptr(spk::Exception("Collection acquisition invalidated")));
		}
	}

private:
	[[nodiscard]] Answer _request(const TKey &key, std::vector<TKey> &missing)
	{
		std::optional<TElement> available;
		{
			const auto reader = _state->storage->read();
			auto found = reader->find(key);
			if (found != reader->end())
			{
				available = found->second;
			}
		}
		if (available.has_value() == true)
		{
			Task task;
			task.validate(std::move(*available));
			return task.answer();
		}
		auto found = _state->pending.find(key);
		if (found != _state->pending.end())
		{
			return found->second.answer;
		}
		auto task = std::make_shared<Task>();
		auto answer = task->answer();
		_state->pending.emplace(key, Pending{answer, {}, task});
		missing.push_back(key);
		return answer;
	}

public:
	Provider() = default;
	Provider(Provider &&) noexcept = default;
	Provider(const Provider &) = delete;
	virtual ~Provider() = default;
	virtual void disconnect()
	{
		std::vector<std::shared_ptr<Task>> invalidated;
		{
			const std::scoped_lock lock(_state->mutex);
			while (_state->pending.empty() == false)
			{
				invalidated.push_back(_invalidate(_state->pending.begin()->first));
			}
		}
		for (const auto &task : invalidated)
		{
			_failInvalidated(task);
		}
		reclaim();
	}
	void reclaim()
	{
		std::vector<typename Answer::CompletionContract> retired;
		{
			const std::scoped_lock lock(_state->mutex);
			retired.swap(_state->retired);
		}
		// Resignation must not hold the acquisition mutex while a source callback is dispatching.
	}
};

template <typename TKey, typename TElement>
class Collection<TKey, TElement>::GeneratingProvider : public Collection<TKey, TElement>::Provider
{
protected:
	[[nodiscard]] Answer _acquire(const TKey &key) override
	{
		// Capture a value-producing operation, never the lifetime of this Provider.
		return _pool.submit(_operation(key));
	}
	[[nodiscard]] virtual std::function<TElement()> _operation(const TKey &key) const = 0;

private:
	spk::WorkerPool &_pool;

public:
	explicit GeneratingProvider(spk::WorkerPool &pool) :
		_pool(pool)
	{
	}
};
