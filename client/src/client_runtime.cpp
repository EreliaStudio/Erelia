#include "erelia/client/client_runtime.hpp"

#include <container/json/reader.hpp>
#include <design_pattern/singleton.hpp>
#include <exception.hpp>
#include <threading/worker_pool.hpp>

#include <filesystem>
#include <utility>

namespace
{
	void validateConfiguration(
		const ClientRuntime::Configuration &configuration)
	{
		if (configuration.address.empty() == true)
		{
			throw spk::Exception(
				"Client Server address cannot be empty");
		}
		if (configuration.port == 0)
		{
			throw spk::Exception(
				"Client Server port cannot be zero");
		}
	}
}

ClientRuntime::Configuration ClientRuntime::Configuration::load(
	const std::string &path)
{
	const std::filesystem::path file(path);
	const spk::JSON::Value document =
		spk::JSON::Loader::parseFile(file);
	const spk::JSON::Reader root(document, file);
	root.forbidUnknown({"server config"});

	const spk::JSON::Reader server =
		root.child("server config");
	server.forbidUnknown({"address", "port"});

	Configuration result{
		.address = server.require<std::string>("address"),
		.port = server.require<std::uint16_t>("port")};
	validateConfiguration(result);
	return result;
}

ClientRuntime::ClientRuntime(Configuration configuration) :
	_configuration(std::move(configuration))
{
	validateConfiguration(_configuration);
}

ClientRuntime::~ClientRuntime()
{
	try
	{
		disconnect();
	} catch (...)
	{
	}
}

ClientRuntime::ConnectionAnswer ClientRuntime::connect()
{
	if (_client.isConnected() == true)
	{
		return _connectionAttempt.value();
	}

	if (
		_connectionAttempt.has_value() == true &&
		_connectionAttempt->status() ==
			ConnectionTask::Status::Pending)
	{
		return *_connectionAttempt;
	}

	spk::WorkerPool &workerPool =
		spk::Singleton<spk::WorkerPool>::instance();
	_connectionAttempt.emplace(
		workerPool.submit(
			[this] {
				_client.connect(
					_configuration.address,
					_configuration.port);
				return true;
			}));

	return *_connectionAttempt;
}

void ClientRuntime::disconnect()
{
	if (
		_connectionAttempt.has_value() == true &&
		_connectionAttempt->status() ==
			ConnectionTask::Status::Pending)
	{
		_connectionAttempt->wait();
	}

	if (_client.isConnected() == true)
	{
		_client.disconnect();
	}
}

bool ClientRuntime::isConnected() const noexcept
{
	return _client.isConnected();
}
