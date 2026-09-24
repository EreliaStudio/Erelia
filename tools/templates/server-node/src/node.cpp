#include "__NODE_SNAKE___node.hpp"

#include <container/json/reader.hpp>

#include <filesystem>
#include <utility>

__NODE_NAME__Node::Configuration __NODE_NAME__Node::Configuration::load(
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

__NODE_NAME__Node::__NODE_NAME__Node(Configuration configuration) :
	_configuration(std::move(configuration))
{
}

__NODE_NAME__Node::~__NODE_NAME__Node()
{
	try
	{
		stop();
	}
	catch (...)
	{
	}
}

void __NODE_NAME__Node::start()
{
	if (isRunning())
	{
		stop();
	}
	_endpoint.start(_configuration.port);
}

void __NODE_NAME__Node::stop()
{
	_endpoint.stop();
}

void __NODE_NAME__Node::dispatch()
{
	if (isRunning())
	{
		_endpoint.dispatch();
	}
}

bool __NODE_NAME__Node::isRunning() const noexcept
{
	return _endpoint.isRunning();
}

std::uint16_t __NODE_NAME__Node::port() const noexcept
{
	return _endpoint.port();
}
