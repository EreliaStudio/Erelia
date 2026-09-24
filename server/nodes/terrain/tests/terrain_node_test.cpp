#include "terrain_node.hpp"

#include <exception.hpp>
#include <gtest/gtest.h>
#include <type/uuid.hpp>

#include <filesystem>
#include <fstream>
#include <string>

namespace
{
	class TemporaryJsonFile
	{
	private:
		std::filesystem::path _path;

	public:
		explicit TemporaryJsonFile(
			const std::string &content)
		{
			_path =
				std::filesystem::temp_directory_path() /
				("erelia-terrain-node-" +
				 spk::UUID::generate().toString() +
				 ".json");
			std::ofstream stream(_path);
			stream << content;
		}

		~TemporaryJsonFile()
		{
			std::error_code error;
			std::filesystem::remove(_path, error);
		}

		[[nodiscard]] const std::filesystem::path &path() const noexcept
		{
			return _path;
		}
	};
}

TEST(TerrainNodeConfiguration, LoadsAndRejectsExactContract)
{
	const TemporaryJsonFile valid(
		R"({"server config":{"port":0}})");
	EXPECT_EQ(
		TerrainNode::Configuration::load(
			valid.path().string()).port,
		0u);

	const std::string invalidFixtures[] = {
		R"({})",
		R"({"server config":{}})",
		R"({"server config":{"port":70000}})",
		R"({"server config":{"port":0,"extra":true}})",
		R"({"server config":{"port":0},"extra":true})"};

	for (const std::string &fixture : invalidFixtures)
	{
		const TemporaryJsonFile file(fixture);
		EXPECT_THROW(
			(void)TerrainNode::Configuration::load(
				file.path().string()),
			spk::Exception);
	}
}

TEST(TerrainNodeRuntime, StartsStopsAndRestarts)
{
	TerrainNode node(
		TerrainNode::Configuration{
			.port = 0});

	node.start();
	EXPECT_TRUE(node.isRunning());
	EXPECT_NE(node.port(), 0u);

	node.stop();
	EXPECT_FALSE(node.isRunning());
	EXPECT_EQ(node.port(), 0u);

	node.start();
	EXPECT_TRUE(node.isRunning());
	EXPECT_NE(node.port(), 0u);
	node.stop();
}
