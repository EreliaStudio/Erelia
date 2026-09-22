#include "voxel/voxel_definition.hpp"

#include <stdexcept>

namespace voxel::Voxel
{
	void Definition::_loadTextures(const spk::JSON::Reader &reader)
	{
		const spk::JSON::Reader texturesReader = reader.child("textures");
		for (const auto &[slot, value] : texturesReader.value().asObject())
			textures.emplace(slot, spk::Vector2UInt(value));
	}

	void Definition::_validateTextureSlots() const
	{
		for (const auto &[slot, cell] : textures)
		{
			(void)cell;
			if (!shape->materialSlots.contains(slot))
				_throw("voxel '" + name + "' has extra texture slot '" + slot + "'");
		}

		for (const std::string &slot : shape->materialSlots)
		{
			if (!textures.contains(slot))
				_throw("voxel '" + name + "' is missing texture slot '" + slot + "'");
		}
	}

	void Definition::_validateTextureCoordinates(const spk::SpriteSheet &atlas) const
	{
		for (const auto &[slot, cell] : textures)
		{
			try
			{
				(void)atlas.sprite(cell);
			}
			catch (const std::out_of_range &)
			{
				_throw("voxel '" + name + "' texture slot '" + slot + "' references an atlas cell out of range");
			}
		}
	}

	void Definition::_resolve(const std::unordered_map<std::string, Shape> &shapes, const spk::SpriteSheet &atlas)
	{
		const auto found = shapes.find(_shapeName);
		if (found == shapes.end())
			_throw("voxel '" + name + "' uses unknown shape '" + _shapeName + "'");

		shape = &found->second;
		_validateTextureSlots();
		_validateTextureCoordinates(atlas);
	}

	[[noreturn]] void Definition::_throw(const std::string &message) const
	{
		throw spk::Exception(_sourceFile.generic_string() + ":" + _sourcePath + ": " + message);
	}

	Definition::Definition(const spk::JSON::Reader &reader) :
		_shapeName(reader.require<std::string>("shape")),
		_sourceFile(reader.file()),
		_sourcePath(reader.path()),
		name(reader.require<std::string>("id"))
	{
		if (name.empty())
			_throw("voxel id cannot be empty");
		if (_shapeName.empty())
			_throw("voxel '" + name + "' shape cannot be empty");
		_loadTextures(reader);
	}
}
