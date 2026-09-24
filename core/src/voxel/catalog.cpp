#include "erelia/core/voxel/catalog.hpp"

#include <cstdint>
#include <exception>
#include <set>
#include <string>
#include <utility>

#include <container/json/reader.hpp>
#include <diagnostics/logger.hpp>

namespace
{
	constexpr std::uint64_t MaximumDefinitionID = 0x1FFFFFFFu;

	[[noreturn]] void throwAt(
		const std::filesystem::path &file,
		const std::string &path,
		const std::string &message)
	{
		throw spk::Exception(file.generic_string() + ":" + path + ": " + message);
	}

	[[nodiscard]] Voxel::Material::ID readMaterialID(
		const spk::JSON::Reader &slotsReader,
		const Voxel::Material::SlotID &slot,
		const spk::JSON::Value &value)
	{
		try
		{
			return value.as<std::string>();
		} catch (...)
		{
			throw spk::Exception(
				slotsReader.file().generic_string() + ":" + slotsReader.pathFor(slot) + ": invalid value",
				std::current_exception());
		}
	}
}

namespace Voxel
{
	Shape::ID Shape::Catalog::_parseKey(const spk::JSON::Reader &reader) const
	{
		const Shape::ID id = reader.require<Shape::ID>("id");
		if (id.empty())
		{
			throw spk::Exception(
				reader.file().generic_string() + ":" + reader.pathFor("id") + ": voxel Shape ID cannot be empty");
		}
		return id;
	}

	Shape Shape::Catalog::_parseElement(const spk::JSON::Reader &reader) const
	{
		return Shape(reader);
	}

	Definition::Catalog::Catalog(const Shape::Catalog &shapes) :
		_shapes(shapes)
	{
		_insert(0u, Definition(_shapes._empty, {}));
	}

	Definition::ID Definition::Catalog::_parseKey(const spk::JSON::Reader &reader) const
	{
		const std::uint64_t authoredID = reader.require<std::uint64_t>("id");
		if (authoredID == 0u)
		{
			throw spk::Exception(
				reader.file().generic_string() + ":" + reader.pathFor("id") + ": voxel Definition ID 0 is reserved for Air");
		}
		if (authoredID > MaximumDefinitionID)
		{
			throw spk::Exception(
				reader.file().generic_string() + ":" + reader.pathFor("id") + ": voxel Definition ID exceeds its 29-bit capacity");
		}
		return static_cast<Definition::ID>(authoredID);
	}

	Definition Definition::Catalog::_parseElement(const spk::JSON::Reader &dataReader) const
	{
		dataReader.forbidUnknown({"shape", "slots"});
		const Shape::ID shapeID = dataReader.require<Shape::ID>("shape");
		if (!_shapes.contains(shapeID))
		{
			throw spk::Exception(
				dataReader.file().generic_string() + ":" + dataReader.pathFor("shape") + ": unknown voxel Shape ID '" + shapeID + "'");
		}

		const Shape &shape = _shapes.at(shapeID);
		std::set<Material::SlotID> shapeSlots;
		for (const Shape::Polygon &polygon : shape.polygons())
		{
			shapeSlots.insert(polygon.slot);
		}

		const spk::JSON::Reader slotsReader = dataReader.child("slots");
		Definition::SlotBindings slots;
		for (const auto &[slot, value] : slotsReader.value().asObject())
		{
			if (slot.empty())
			{
				throwAt(slotsReader.file(), slotsReader.path(), "voxel Definition slot cannot be empty");
			}
			if (!shapeSlots.contains(slot))
			{
				throw spk::Exception(
					slotsReader.file().generic_string() + ":" + slotsReader.pathFor(slot) + ": voxel Definition has extra slot '" + slot + "'");
			}
			slots.emplace(slot, readMaterialID(slotsReader, slot, value));
		}

		for (const Material::SlotID &slot : shapeSlots)
		{
			if (slots.contains(slot))
			{
				continue;
			}

			SPK_LOG(Warning)
				<< dataReader.file().generic_string() << ':' << dataReader.path()
				<< ": voxel Definition is missing slot '" << slot
				<< "'; binding Material::InvalidID" << std::endl;
			slots.emplace(slot, Material::InvalidID);
		}

		return Definition(shape, std::move(slots));
	}

	Catalog::Catalog() :
		_shapes(),
		_definitions(_shapes)
	{
	}

	void Catalog::load(const std::filesystem::path &shapePath, const std::filesystem::path &definitionPath)
	{
		loadShape(shapePath);
		loadDefinition(definitionPath);
	}

	void Catalog::loadShape(const std::filesystem::path &path)
	{
		_shapes.load(path);
	}

	void Catalog::loadDefinition(const std::filesystem::path &path)
	{
		_definitions.load(path);
	}

	const Shape::Catalog &Catalog::shapes() const noexcept
	{
		return _shapes;
	}

	const Definition::Catalog &Catalog::definitions() const noexcept
	{
		return _definitions;
	}
}
