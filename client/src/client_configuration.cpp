#include "erelia/client/client_configuration.hpp"

#include <container/json/reader.hpp>
#include <exception.hpp>

#include <cstdint>
#include <filesystem>

ClientConfiguration ClientConfiguration::load(const std::string &path)
{
	const std::filesystem::path file(path);
	const spk::JSON::Value document = spk::JSON::Loader::parseFile(file);
	const spk::JSON::Reader root(document, file);
	root.forbidUnknown({"server config"});

	const spk::JSON::Reader server = root.child("server config");
	server.forbidUnknown({"address", "port", "retryDelayMs"});

	ClientConfiguration result{
		.server = {
			.address = server.require<std::string>("address"),
			.port = server.require<std::uint16_t>("port")},
		.retryDelay = std::chrono::milliseconds(
			server.require<std::uint32_t>("retryDelayMs"))};

	if (result.server.address.empty() == true)
	{
		throw spk::Exception("Client Server address cannot be empty");
	}
	if (result.server.port == 0)
	{
		throw spk::Exception("Client Server port cannot be zero");
	}
	if (result.retryDelay.count() <= 0)
	{
		throw spk::Exception("Client retryDelayMs must be greater than zero");
	}
	return result;
}
