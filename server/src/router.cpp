#include "erelia/server/router.hpp"

#include <container/json/reader.hpp>
#include <diagnostics/logger.hpp>
#include <exception.hpp>
#include <network/remote_node.hpp>

#include <chrono>
#include <cstdint>
#include <filesystem>
#include <limits>
#include <string>
#include <unordered_set>
#include <utility>

namespace
{
	void validateConfiguration(
		const Router::Configuration &configuration)
	{
		if (configuration.nodeReconnectDelay.count() <= 0)
		{
			throw spk::Exception(
				"Router nodeReconnectDelayMs must be greater than zero");
		}

		std::unordered_set<std::string> names;
		for (const Router::NodeConfiguration &node : configuration.nodes)
		{
			if (node.name.empty())
			{
				throw spk::Exception(
					"Router node name cannot be empty");
			}
			if (node.address.empty())
			{
				throw spk::Exception(
					"Router node address cannot be empty");
			}
			if (node.port == 0)
			{
				throw spk::Exception(
					"Router remote node port cannot be zero");
			}
			if (!names.emplace(node.name).second)
			{
				throw spk::Exception(
					"Duplicate Router node name: " + node.name);
			}
		}
	}
}

struct Router::NodeState
{
	NodeConfiguration configuration;
	spk::RemoteNode remote;
	std::chrono::steady_clock::time_point nextConnectionAttempt =
		std::chrono::steady_clock::time_point::min();

	explicit NodeState(NodeConfiguration value) :
		configuration(std::move(value))
	{
	}
};

Router::Configuration Router::Configuration::load(
	const std::string &path)
{
	const std::filesystem::path file(path);
	const spk::JSON::Value document =
		spk::JSON::Loader::parseFile(file);
	const spk::JSON::Reader root(document, file);
	root.forbidUnknown({"server config", "nodes"});

	const spk::JSON::Reader server =
		root.child("server config");
	server.forbidUnknown({"port", "nodeReconnectDelayMs"});

	Configuration result;
	result.port = server.require<std::uint16_t>("port");

	const std::uint32_t reconnectDelay =
		server.require<std::uint32_t>("nodeReconnectDelayMs");
	result.nodeReconnectDelay =
		std::chrono::milliseconds(reconnectDelay);

	for (const spk::JSON::Reader &nodeReader :
		 root.childArray("nodes"))
	{
		nodeReader.forbidUnknown(
			{"name", "address", "port"});

		result.nodes.push_back(
			NodeConfiguration{
				.name = nodeReader.require<std::string>("name"),
				.address = nodeReader.require<std::string>("address"),
				.port = nodeReader.require<std::uint16_t>("port")});
	}

	validateConfiguration(result);
	return result;
}

Router::Router(Configuration configuration) :
	_configuration(std::move(configuration))
{
	validateConfiguration(_configuration);

	_nodes.reserve(_configuration.nodes.size());
	for (const NodeConfiguration &nodeConfiguration :
		 _configuration.nodes)
	{
		auto node =
			std::make_unique<NodeState>(nodeConfiguration);
		_router.addNode(
			node->configuration.name,
			node->remote);
		_nodes.push_back(std::move(node));
	}
}

Router::~Router()
{
	try
	{
		stop();
	}
	catch (...)
	{
	}
}

void Router::_attemptConnection(
	NodeState &node,
	std::chrono::steady_clock::time_point now)
{
	try
	{
		node.remote.connect(
			node.configuration.address,
			node.configuration.port);
		node.nextConnectionAttempt =
			std::chrono::steady_clock::time_point::max();
	}
	catch (const std::exception &exception)
	{
		SPK_LOG(Warning)
			<< "Unable to connect Server node '"
			<< node.configuration.name
			<< "' at "
			<< node.configuration.address
			<< ':'
			<< node.configuration.port
			<< ": "
			<< exception.what()
			<< std::endl;

		node.nextConnectionAttempt =
			now + _configuration.nodeReconnectDelay;
	}
}

void Router::start()
{
	if (isRunning())
	{
		stop();
	}

	_router.start(_configuration.port);

	const auto now = std::chrono::steady_clock::now();
	for (const auto &node : _nodes)
	{
		node->nextConnectionAttempt = now;
		_attemptConnection(*node, now);
	}
}

void Router::stop()
{
	for (const auto &node : _nodes)
	{
		if (node->remote.isConnected())
		{
			node->remote.disconnect();
		}
		node->nextConnectionAttempt =
			std::chrono::steady_clock::time_point::min();
	}

	_router.stop();
}

void Router::dispatch()
{
	if (!isRunning())
	{
		return;
	}

	const auto now = std::chrono::steady_clock::now();
	for (const auto &node : _nodes)
	{
		if (node->remote.isConnected())
		{
			continue;
		}

		if (node->nextConnectionAttempt ==
			std::chrono::steady_clock::time_point::max())
		{
			node->nextConnectionAttempt = now;
		}

		if (now >= node->nextConnectionAttempt)
		{
			_attemptConnection(*node, now);
		}
	}

	_router.dispatch();

	for (const auto &node : _nodes)
	{
		if (node->remote.isConnected())
		{
			node->remote.dispatch();
		}
	}
}

bool Router::isRunning() const noexcept
{
	return _router.server().isRunning();
}

std::uint16_t Router::port() const noexcept
{
	return _router.server().port();
}

bool Router::isNodeConnected(
	std::string_view name) const noexcept
{
	for (const auto &node : _nodes)
	{
		if (node->configuration.name == name)
		{
			return node->remote.isConnected();
		}
	}
	return false;
}

void Router::redirect(
	spk::Message::Type messageType,
	std::string_view nodeName)
{
	_router.redirect(messageType, nodeName);
}
