#include "terrain_node.hpp"

#include "prototype_chunk_provider.hpp"

#include <container/json/reader.hpp>

#include <filesystem>
#include <utility>

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
	_chunks(PrototypeChunkProvider{})
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
}

void TerrainNode::stop()
{
	_endpoint.stop();
}

void TerrainNode::dispatch()
{
	if (isRunning())
	{
		_endpoint.dispatch();
		_chunks.update();
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
