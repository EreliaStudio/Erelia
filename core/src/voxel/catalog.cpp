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
		const std::string &slot,
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
	Shape::Catalog::Catalog() :
		Base(
			[](const spk::JSON::Reader &elementReader) -> Shape::ID {
				const Shape::ID id = elementReader.require<Shape::ID>("id");
				if (id.empty())
				{
					throw spk::Exception(
						elementReader.file().generic_string() + ":" + elementReader.pathFor("id") + ": voxel Shape ID cannot be empty");
				}
				return id;
			},
			[](const spk::JSON::Reader &dataReader, const Shape::ID &) -> std::shared_ptr<const Shape> {
				return std::shared_ptr<const Shape>(new Shape(dataReader));
			})
	{
	}

	std::shared_ptr<const Shape> Shape::Catalog::_sharedShape(const Shape::ID &id) const
	{
		return Base::_sharedAt(id);
	}

	void Shape::Catalog::_load(const std::filesystem::path &path)
	{
		Base::_load(path);
	}

	Definition::Catalog::Catalog(const Shape::Catalog &shapes) :
		Base(
			[](const spk::JSON::Reader &elementReader, const Shape::Catalog &) -> Definition::ID {
				const std::uint64_t authoredID = elementReader.require<std::uint64_t>("id");
				if (authoredID == 0u)
				{
					throw spk::Exception(
						elementReader.file().generic_string() + ":" + elementReader.pathFor("id") + ": voxel Definition ID 0 is reserved for Air");
				}
				if (authoredID > MaximumDefinitionID)
				{
					throw spk::Exception(
						elementReader.file().generic_string() + ":" + elementReader.pathFor("id") + ": voxel Definition ID exceeds its 29-bit capacity");
				}
				return static_cast<Definition::ID>(authoredID);
			},
			[](const spk::JSON::Reader &dataReader, const Definition::ID &id, const Shape::Catalog &shapes)
				-> std::shared_ptr<const Definition> {
				dataReader.forbidUnknown({"shape", "slots"});
				const Shape::ID shapeID = dataReader.require<Shape::ID>("shape");
				if (!shapes.contains(shapeID))
				{
					throw spk::Exception(
						dataReader.file().generic_string() + ":" + dataReader.pathFor("shape") + ": unknown voxel Shape ID '" + shapeID + "'");
				}

				const std::shared_ptr<const Shape> shape = shapes._sharedShape(shapeID);
				std::set<std::string> shapeSlots;
				for (const Shape::Polygon &polygon : shape->polygons())
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

				for (const std::string &slot : shapeSlots)
				{
					if (slots.contains(slot))
					{
						continue;
					}

					SPK_LOG(Warning)
						<< dataReader.file().generic_string() << ':' << dataReader.path()
						<< ": voxel Definition " << id << " is missing slot '" << slot
						<< "'; binding Material::InvalidID" << std::endl;
					slots.emplace(slot, Material::InvalidID);
				}

				return std::shared_ptr<const Definition>(new Definition(shape, std::move(slots)));
			}),
		_shapes(&shapes)
	{
		Base::_insert(0u, std::shared_ptr<const Definition>(new Definition()));
	}

	void Definition::Catalog::_load(const std::filesystem::path &path)
	{
		Base::_load(path, *_shapes);
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
		_shapes._load(path);
	}

	void Catalog::loadDefinition(const std::filesystem::path &path)
	{
		_definitions._load(path);
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
