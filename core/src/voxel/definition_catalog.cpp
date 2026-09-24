#include "erelia/core/voxel/catalog.hpp"

#include <cstdint>
#include <exception>
#include <set>
#include <string>
#include <utility>

#include <container/json/error.hpp>
#include <container/json/reader.hpp>
#include <diagnostics/logger.hpp>

namespace
{
	constexpr std::uint64_t MaximumDefinitionID = 0x1FFFFFFFu;

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
			spk::JSON::throwAt(
				slotsReader.file(),
				slotsReader.pathFor(slot),
				"invalid value",
				std::current_exception());
		}
	}
}

namespace Voxel
{
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
			spk::JSON::throwAt(reader.file(), reader.pathFor("id"), "voxel Definition ID 0 is reserved for Air");
		}
		if (authoredID > MaximumDefinitionID)
		{
			spk::JSON::throwAt(reader.file(), reader.pathFor("id"), "voxel Definition ID exceeds its 29-bit capacity");
		}
		return static_cast<Definition::ID>(authoredID);
	}

	Definition Definition::Catalog::_parseElement(const spk::JSON::Reader &dataReader) const
	{
		dataReader.forbidUnknown({"shape", "slots"});
		const Shape::ID shapeID = dataReader.require<Shape::ID>("shape");
		if (!_shapes.contains(shapeID))
		{
			spk::JSON::throwAt(
				dataReader.file(),
				dataReader.pathFor("shape"),
				"unknown voxel Shape ID '" + shapeID + "'");
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
				spk::JSON::throwAt(slotsReader.file(), slotsReader.path(), "voxel Definition slot cannot be empty");
			}
			if (!shapeSlots.contains(slot))
			{
				spk::JSON::throwAt(
					slotsReader.file(),
					slotsReader.pathFor(slot),
					"voxel Definition has extra slot '" + slot + "'");
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
}
