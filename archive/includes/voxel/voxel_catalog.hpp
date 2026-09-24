#pragma once

#include <concepts>
#include <filesystem>
#include <exception.hpp>
#include <string>
#include <type_traits>
#include <unordered_map>
#include <utility>
#include <vector>

#include <sparkle>

#include "voxel/voxel_cell.hpp"
#include "voxel/voxel_definition.hpp"

namespace voxel::Voxel
{
	template <typename TDefinition>
	class Catalog final
	{
		static_assert(std::derived_from<TDefinition, Definition>);
		static_assert(std::constructible_from<TDefinition, const spk::JSON::Reader &>);

	private:
		struct Configuration
		{
			std::filesystem::path atlasFile;
			spk::Vector2UInt atlasGrid;
			std::filesystem::path shapesFile;
			std::filesystem::path definitionsFile;
		};

		spk::SpriteSheet _atlas;
		std::unordered_map<std::string, Shape> _shapes;
		std::vector<TDefinition> _definitions;
		std::unordered_map<std::string, ID> _ids;

		[[nodiscard]] static Configuration _loadConfiguration(const std::filesystem::path &catalogFile)
		{
			const auto value = spk::JSON::Loader::parseFile(catalogFile);
			spk::JSON::Reader reader(value, catalogFile);
			reader.forbidUnknown({"atlas", "shapes", "definitions"});

			const spk::JSON::Reader atlasReader = reader.child("atlas");
			atlasReader.forbidUnknown({"texture", "grid"});
			return {
				atlasReader.require<std::string>("texture"),
				spk::Vector2UInt(atlasReader.value().at("grid")),
				reader.require<std::string>("shapes"),
				reader.require<std::string>("definitions")};
		}

		void _insertShape(Shape shape)
		{
			if (_shapes.contains(shape.name))
				throw spk::Exception("duplicate voxel shape id '" + shape.name + "'");
			_shapes.emplace(shape.name, std::move(shape));
		}

		void _loadShapes(const std::filesystem::path &file)
		{
			const auto values = spk::JSON::Loader::parseFile(file);
			std::size_t index = 0;
			for (const auto &value : values.asArray())
			{
				spk::JSON::Reader reader(value, file, "$[" + std::to_string(index++) + "]");
				_insertShape(Shape(reader));
			}
		}

		void _appendDefinition(TDefinition definition)
		{
			Definition &base = definition;
			if (_ids.contains(base.name))
				throw spk::Exception("duplicate voxel id '" + base.name + "'");
			if (_definitions.size() >= Cell::IDMask)
				throw spk::Exception("voxel catalog exceeds packed identifier capacity");

			base._resolve(_shapes, _atlas);
			base.id = static_cast<ID>(_definitions.size() + 1);
			_ids.emplace(base.name, base.id);
			_definitions.push_back(std::move(definition));
		}

		void _loadDefinitions(const std::filesystem::path &file)
		{
			const auto values = spk::JSON::Loader::parseFile(file);
			std::size_t index = 0;
			for (const auto &value : values.asArray())
			{
				spk::JSON::Reader reader(value, file, "$[" + std::to_string(index++) + "]");
				_appendDefinition(TDefinition(reader));
			}
		}

	public:
		Catalog() = default;
		Catalog(const Catalog &) = delete;
		Catalog &operator=(const Catalog &) = delete;
		Catalog(Catalog &&) noexcept = default;
		Catalog &operator=(Catalog &&) noexcept = default;

		[[nodiscard]] static Catalog load(const std::filesystem::path &catalogFile)
		{
			const Configuration configuration = _loadConfiguration(catalogFile);
			Catalog result;
			result._atlas = spk::SpriteSheet::open(configuration.atlasFile, configuration.atlasGrid);
			result._loadShapes(configuration.shapesFile);
			result._loadDefinitions(configuration.definitionsFile);
			return result;
		}

		[[nodiscard]] const TDefinition &definition(ID id) const
		{
			if (id == 0 || id > _definitions.size())
				throw spk::Exception("unknown voxel runtime id " + std::to_string(id));
			return _definitions[id - 1];
		}

		[[nodiscard]] const TDefinition &definition(const std::string &name) const
		{
			return definition(id(name));
		}

		[[nodiscard]] ID id(const std::string &name) const
		{
			const auto found = _ids.find(name);
			if (found == _ids.end())
				throw spk::Exception("unknown voxel '" + name + "'");
			return found->second;
		}

		[[nodiscard]] const spk::SpriteSheet &atlas() const noexcept
		{
			return _atlas;
		}

		[[nodiscard]] std::size_t size() const noexcept
		{
			return _definitions.size();
		}
	};
}
