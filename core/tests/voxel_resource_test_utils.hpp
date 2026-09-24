#pragma once

#include <cstdint>
#include <filesystem>
#include <fstream>
#include <string>
#include <utility>

#include <type/uuid.hpp>

namespace voxel_test
{
	class TemporaryJsonFile
	{
	private:
		std::filesystem::path _path;

	public:
		explicit TemporaryJsonFile(std::string content, const std::string &label = "resource") :
			_path(
				std::filesystem::temp_directory_path() /
				("erelia-" + label + "-" + spk::UUID::generate().toString() + ".json"))
		{
			std::ofstream stream(_path, std::ios::binary);
			stream << content;
		}

		TemporaryJsonFile(const TemporaryJsonFile &) = delete;
		TemporaryJsonFile &operator=(const TemporaryJsonFile &) = delete;
		TemporaryJsonFile(TemporaryJsonFile &&) = delete;
		TemporaryJsonFile &operator=(TemporaryJsonFile &&) = delete;

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

	[[nodiscard]] inline std::filesystem::path voxelResourcePath(
		const std::filesystem::path &relative)
	{
		return std::filesystem::path("resources") / "voxels" / relative;
	}

	[[nodiscard]] inline std::filesystem::path shapeResourcePath()
	{
		return voxelResourcePath("shapes.json");
	}

	[[nodiscard]] inline std::filesystem::path slopeShapeResourcePath()
	{
		return voxelResourcePath(std::filesystem::path("shapes") / "slope.json");
	}

	[[nodiscard]] inline std::filesystem::path stairShapeResourcePath()
	{
		return voxelResourcePath(std::filesystem::path("shapes") / "stair.json");
	}

	[[nodiscard]] inline std::filesystem::path definitionResourcePath()
	{
		return voxelResourcePath("definition.json");
	}

	[[nodiscard]] inline std::filesystem::path slopeDefinitionResourcePath()
	{
		return voxelResourcePath(std::filesystem::path("definitions") / "slope.json");
	}

	[[nodiscard]] inline std::filesystem::path stairDefinitionResourcePath()
	{
		return voxelResourcePath(std::filesystem::path("definitions") / "stair.json");
	}

	[[nodiscard]] inline std::string shapeFile(
		const std::string &id,
		const std::string &polygonJson)
	{
		return R"({"elements":[{"id":")" + id + R"(","data":{"polygons":[)" + polygonJson + R"(]}}]})";
	}

	[[nodiscard]] inline std::string definitionFile(
		std::uint32_t id,
		const std::string &shape,
		const std::string &slotsJson)
	{
		return R"({"elements":[{"id":)" + std::to_string(id) + R"(,"data":{"shape":")" + shape + R"(","slots":)" + slotsJson + R"(}}]})";
	}
}
