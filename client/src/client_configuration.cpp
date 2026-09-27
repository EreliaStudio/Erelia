#include "erelia/client/client_configuration.hpp"

#include "erelia/client/service.hpp"

#include <container/json/reader.hpp>
#include <exception.hpp>
#include <system/translator.hpp>

#include <cstdint>
#include <filesystem>

ClientConfiguration ClientConfiguration::load(const std::filesystem::path &path)
{
	const spk::JSON::Value document = spk::JSON::Loader::parseFile(path);
	const spk::JSON::Reader root(document, path);
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
		throw spk::Exception(
			Service::translator()->translate(
				"client.connection.endpoint.address_empty"));
	}
	if (result.server.port == 0)
	{
		throw spk::Exception(
			Service::translator()->translate(
				"client.connection.endpoint.port_zero"));
	}
	if (result.retryDelay.count() <= 0)
	{
		throw spk::Exception(
			Service::translator()->translate(
				"client.configuration.retry_delay_non_positive"));
	}
	return result;
}
