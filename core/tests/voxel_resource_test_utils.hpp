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
			_path(std::filesystem::temp_directory_path() /
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

	[[nodiscard]] inline std::filesystem::path shapeResourcePath()
	{
		return std::filesystem::path(ERELIA_SOURCE_DIR) / "resources" / "voxels" / "shapes.json";
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
