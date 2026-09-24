#pragma once

#include <filesystem>
#include <string>
#include <unordered_map>

#include <sparkle>

#include "voxel/voxel_shape.hpp"

namespace voxel::Voxel
{
	struct Definition
	{
	private:
		template <typename TDefinition>
		friend class Catalog;

		std::string _shapeName;
		std::filesystem::path _sourceFile;
		std::string _sourcePath;

		void _loadTextures(const spk::JSON::Reader &reader);
		void _resolve(const std::unordered_map<std::string, Shape> &shapes, const spk::SpriteSheet &atlas);
		void _validateTextureSlots() const;
		void _validateTextureCoordinates(const spk::SpriteSheet &atlas) const;
		[[noreturn]] void _throw(const std::string &message) const;

	public:
		ID id = 0;
		std::string name;
		const Shape *shape = nullptr;
		std::unordered_map<std::string, spk::Vector2UInt> textures;

		explicit Definition(const spk::JSON::Reader &reader);
	};
}
