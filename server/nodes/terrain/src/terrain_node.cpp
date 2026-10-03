#include "terrain_node.hpp"
#include "terrain_service.hpp"
#include <atomic>
#include <container/json/reader.hpp>
#include <container/thread_safe_fifo.hpp>
#include <diagnostics/logger.hpp>
#include <optional>
#include <set>
namespace
{
	std::atomic_bool drainerOwned = false;
	std::string failureMessage(std::exception_ptr failure)
	{
		try
		{
			std::rethrow_exception(failure);
		} catch (const spk::Exception &error)
		{
			return error.message();
		} catch (const std::exception &error)
		{
			return error.what();
		} catch (...)
		{
			return "Unknown acquisition failure";
		}
	}
}
struct TerrainNode::AsyncState
{
	struct Context
	{
		spk::ContractProvider<>::Contract contract;
	};
	struct Reply
	{
		Request request;
		spk::Message message;
	};
	spk::ThreadSafeFIFO<Reply> replies;
	std::vector<Reply> drained;
	std::vector<std::shared_ptr<Context>> outstanding;
};
TerrainNode::Configuration TerrainNode::Configuration::load(const std::string &path)
{
	const std::filesystem::path file(path);
	const auto document = spk::JSON::Loader::parseFile(file);
	const spk::JSON::Reader root(document, file);
	root.forbidUnknown({"server config"});
	const auto server = root.child("server config");
	server.forbidUnknown({"port"});
	return {.port = server.require<std::uint16_t>("port")};
}
TerrainNode::TerrainNode(Configuration configuration) :
	_configuration(configuration),
	_endpoint(Service::terrainEndpoint()),
	_async(std::make_unique<AsyncState>())
{
	if (drainerOwned.exchange(true) == true)
	{
		throw spk::Exception("Terrain Endpoint already has a dispatcher");
	}
	try
	{
		_subscriptions.push_back(_dispatcher.subscribe(static_cast<spk::Message::Type>(Networking::MessageType::ChunkRequest), [this](const Request &request) {
			_request(request, *_world.chunkCollection());
		}));
		_subscriptions.push_back(_dispatcher.subscribe(static_cast<spk::Message::Type>(Networking::MessageType::ColumnRequest), [this](const Request &request) {
			_request(request, *_world.columnCollection());
		}));
	} catch (...)
	{
		drainerOwned.store(false);
		throw;
	}
}
TerrainNode::~TerrainNode()
{
	try
	{
		stop();
	} catch (...)
	{
	}
	drainerOwned.store(false);
}
void TerrainNode::start()
{
	stop();
	_endpoint.start(_configuration.port);
}
void TerrainNode::stop()
{
	_endpoint.stop();
	_async = std::make_unique<AsyncState>();
	_requests.clear();
}
void TerrainNode::dispatch()
{
	if (isRunning() == false)
	{
		return;
	}
	_endpoint.dispatch();
	for (const auto &request : _endpoint.requests().drain(_requests))
	{
		_dispatcher.dispatch(request.message.type(), request);
	}
	_requests.clear();
	_drainCompletions();
}
template <typename TKey, typename TElement>
void TerrainNode::_request(const Request &request, Collection<TKey, TElement> &collection)
{
	using Protocol = Networking::CollectionProtocol::Codec<TKey, TElement>;
	const std::string domain = std::is_same_v<TElement, Chunk> == true ? "Chunk" : "Column";
	std::vector<TKey> keys;
	try
	{
		keys = typename Protocol::Request(request.message).keys();
	} catch (const spk::Exception &)
	{
		reply(request, Protocol::Error::build(request.message.requestID(), {Networking::Diagnostic::Severity::Error, domain + "_Request_Malformed"}));
		return;
	}
	std::set<TKey> seen, duplicated;
	std::vector<TKey> distinct;
	for (const auto &key : keys)
	{
		if (seen.insert(key).second == true)
		{
			distinct.push_back(key);
		}
		else
		{
			duplicated.insert(key);
		}
	}
	if (duplicated.empty() == false)
	{
		reply(request, Protocol::Error::build(request.message.requestID(), {Networking::Diagnostic::Severity::Warning, domain + "_Coordinates_Duplication"}, {duplicated.begin(), duplicated.end()}));
	}
	try
	{
		auto answer = collection.request(distinct);
		auto context = std::make_shared<AsyncState::Context>();
		auto producer = _async->replies.producer();
		_async->outstanding.push_back(context);
		context->contract = answer.subscribeToCompletion([producer, answer, request, distinct, domain]() mutable {
			try
			{
				std::vector<typename Protocol::Success> success;
				std::vector<typename Protocol::Failed> failed;
				for (std::size_t index = 0; index < distinct.size(); ++index)
				{
					const auto &child = answer.at(index);
					if (child.status() == spk::Task<TElement>::Status::Completed)
					{
						success.push_back({distinct[index], child.result()});
					}
					else
					{
						failed.push_back({distinct[index], {Protocol::Failure::Code::AcquisitionFailed, failureMessage(child.failure())}});
					}
				}
				producer.publish({request, Protocol::Response::build(request.message.requestID(), std::move(success), std::move(failed))});
			} catch (...)
			{
				producer.publish({request, Protocol::Error::build(request.message.requestID(), {Networking::Diagnostic::Severity::Error, domain + "_Request_Aggregation_Failure"})});
			}
		});
	} catch (...)
	{
		reply(request, Protocol::Error::build(request.message.requestID(), {Networking::Diagnostic::Severity::Error, domain + "_Request_Aggregation_Failure"}));
	}
}
void TerrainNode::_drainCompletions()
{
	for (const auto &result : _async->replies.drain(_async->drained))
	{
		reply(result.request, result.message);
	}
	_async->drained.clear();
	std::erase_if(_async->outstanding, [](const auto &context) {
		return context->contract.isValid() == false;
	});
}
void TerrainNode::reply(const Request &request, spk::Message message) noexcept
{
	try
	{
		_endpoint.reply(request, std::move(message));
	} catch (const std::exception &error)
	{
		SPK_LOG(Warning) << "Unable to reply from TerrainNode: " << error.what() << std::endl;
	} catch (...)
	{
		SPK_LOG(Warning) << "Unable to reply from TerrainNode" << std::endl;
	}
}
bool TerrainNode::isRunning() const noexcept
{
	return _endpoint.isRunning();
}
std::uint16_t TerrainNode::port() const noexcept
{
	return _endpoint.port();
}
