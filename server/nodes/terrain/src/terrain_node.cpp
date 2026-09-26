#include "terrain_node.hpp"

#include "prototype_chunk_provider.hpp"

#include "erelia/core/chunk_protocol_response.hpp"
#include "erelia/core/networking/diagnostic.hpp"

#include <container/json/reader.hpp>
#include <container/thread_safe_fifo.hpp>
#include <diagnostics/logger.hpp>
#include <exception.hpp>
#include <threading/task_group.hpp>

#include <algorithm>
#include <cstddef>
#include <exception>
#include <filesystem>
#include <memory>
#include <mutex>
#include <optional>
#include <string>
#include <string_view>
#include <utility>
#include <vector>

namespace
{
	constexpr std::size_t ChunkBatchSize = 1024u;
	constexpr std::string_view AggregationFailureKey =
		"Chunk_Request_Aggregation_Failure";

	using BatchResult = Chunk::Collection::BatchResult;
	using BatchTask = spk::Task<BatchResult>;
	using BatchGroup = spk::TaskGroup<BatchResult>;
	using BatchGroupAnswer = BatchGroup::Answer;

	[[nodiscard]] std::string failureMessage(
		const std::exception_ptr &exception)
	{
		if (exception == nullptr)
		{
			return "Unknown acquisition failure";
		}

		try
		{
			std::rethrow_exception(exception);
		} catch (const spk::Exception &current)
		{
			return current.message();
		} catch (const std::exception &current)
		{
			return current.what();
		} catch (...)
		{
			return "Unknown acquisition failure";
		}
	}
}

struct TerrainNode::AsyncState
{
	struct RequestContext final
	{
		Request request;
		BatchGroupAnswer answer;
		std::optional<BatchGroupAnswer::CompletionContract> contract;

		RequestContext(
			Request requestValue,
			BatchGroupAnswer answerValue) :
			request(std::move(requestValue)),
			answer(std::move(answerValue))
		{
		}
	};

	using CompletionQueue =
		spk::ThreadSafeFIFO<std::shared_ptr<RequestContext>>;

	struct CompletionMailbox final
	{
		std::mutex mutex;
		bool accepting = false;
		CompletionQueue::Producer producer;

		explicit CompletionMailbox(
			CompletionQueue::Producer producerValue) :
			producer(std::move(producerValue))
		{
		}

		void activate()
		{
			const std::scoped_lock lock(mutex);
			accepting = true;
		}

		void deactivate()
		{
			const std::scoped_lock lock(mutex);
			accepting = false;
		}

		void publish(
			const std::shared_ptr<RequestContext> &context)
		{
			const std::scoped_lock lock(mutex);
			if (accepting)
			{
				producer.publish(context);
			}
		}
	};

	CompletionQueue completions;
	std::shared_ptr<CompletionMailbox> mailbox =
		std::make_shared<CompletionMailbox>(
			completions.producer());
	std::vector<std::shared_ptr<RequestContext>> outstanding;
	std::vector<std::shared_ptr<RequestContext>> drained;

	void activate()
	{
		mailbox->activate();
	}

	void deactivate()
	{
		mailbox->deactivate();
		outstanding.clear();
		(void)completions.drain(drained);
		drained.clear();
	}

	void remove(
		const std::shared_ptr<RequestContext> &context)
	{
		std::erase(outstanding, context);
	}
};

TerrainNode::Configuration TerrainNode::Configuration::load(
	const std::string &path)
{
	const std::filesystem::path file(path);
	const spk::JSON::Value document =
		spk::JSON::Loader::parseFile(file);
	const spk::JSON::Reader root(document, file);
	root.forbidUnknown({"server config"});

	const spk::JSON::Reader server =
		root.child("server config");
	server.forbidUnknown({"port"});

	return Configuration{
		.port = server.require<std::uint16_t>("port")};
}

TerrainNode::TerrainNode(Configuration configuration) :
	_configuration(std::move(configuration)),
	_chunks(PrototypeChunkProvider{}),
	_async(std::make_unique<AsyncState>())
{
}

TerrainNode::~TerrainNode()
{
	try
	{
		stop();
	} catch (...)
	{
	}
}

void TerrainNode::start()
{
	if (isRunning())
	{
		stop();
	}

	_endpoint.start(_configuration.port);
	_async->activate();
}

void TerrainNode::stop()
{
	_async->deactivate();
	_endpoint.stop();
}

void TerrainNode::dispatch()
{
	if (!isRunning())
	{
		return;
	}

	_endpoint.dispatch();
	_drainCompletions();
}

void TerrainNode::requestChunks(
	Request request,
	std::vector<Chunk::Coordinate> coordinates)
{
	BatchGroup group;

	for (
		std::size_t offset = 0u;
		offset < coordinates.size();
		offset += ChunkBatchSize)
	{
		const std::size_t end =
			std::min(
				coordinates.size(),
				offset + ChunkBatchSize);
		std::vector<Chunk::Coordinate> batch(
			coordinates.begin() +
				static_cast<std::ptrdiff_t>(offset),
			coordinates.begin() +
				static_cast<std::ptrdiff_t>(end));
		group.add(_chunks.request(batch));
	}

	auto context =
		std::make_shared<AsyncState::RequestContext>(
			std::move(request),
			std::move(group).answer());
	_async->outstanding.push_back(context);

	const std::weak_ptr<AsyncState::RequestContext> weakContext =
		context;
	const std::shared_ptr<AsyncState::CompletionMailbox> mailbox =
		_async->mailbox;

	context->contract.emplace(
		context->answer.subscribeToCompletion(
			[weakContext, mailbox] {
				if (const auto current = weakContext.lock();
					current != nullptr)
				{
					mailbox->publish(current);
				}
			}));
}

void TerrainNode::reply(
	const Request &request,
	spk::Message message) noexcept
{
	try
	{
		_endpoint.reply(
			request,
			std::move(message));
	} catch (const std::exception &exception)
	{
		SPK_LOG(Error)
			<< "Unable to reply from TerrainNode: "
			<< exception.what()
			<< std::endl;
	} catch (...)
	{
		SPK_LOG(Error)
			<< "Unable to reply from TerrainNode: unknown exception"
			<< std::endl;
	}
}

void TerrainNode::_drainCompletions()
{
	for (const auto &context :
		 _async->completions.drain(_async->drained))
	{
		if (
			context->answer.status() ==
			BatchTask::Status::Failed)
		{
			Networking::Diagnostic::Builder builder(
				Networking::Diagnostic::Severity::Error,
				std::string(AggregationFailureKey),
				context->request.message.requestID());
			reply(
				context->request,
				std::move(builder).build());
			_async->remove(context);
			continue;
		}

		try
		{
			Chunk::Protocol::Response::Builder builder(
				context->request.message.requestID());

			for (const BatchTask::Answer &batch :
				 context->answer.answers())
			{
				const BatchResult &result =
					batch.result();

				for (const BatchResult::Acquired &acquired :
					 result.acquired)
				{
					builder.addSuccess(
						acquired.coordinate,
						acquired.chunk);
				}

				for (const BatchResult::Failed &failed :
					 result.failed)
				{
					builder.addFailure(
						failed.coordinate,
						Chunk::Protocol::Response::Failure::Code::
							AcquisitionFailed,
						failureMessage(failed.exception));
				}
			}

			reply(
				context->request,
				std::move(builder).build());
		} catch (const std::exception &exception)
		{
			SPK_LOG(Error)
				<< "Unable to build TerrainNode Chunk response: "
				<< exception.what()
				<< std::endl;
		} catch (...)
		{
			SPK_LOG(Error)
				<< "Unable to build TerrainNode Chunk response: unknown exception"
				<< std::endl;
		}

		_async->remove(context);
	}

	_async->drained.clear();
}

bool TerrainNode::isRunning() const noexcept
{
	return _endpoint.isRunning();
}

std::uint16_t TerrainNode::port() const noexcept
{
	return _endpoint.port();
}

TerrainNode::RequestQueue &TerrainNode::requests() noexcept
{
	return _endpoint.requests();
}
