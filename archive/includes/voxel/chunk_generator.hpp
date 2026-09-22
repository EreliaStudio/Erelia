#pragma once

#include <memory>

#include "voxel/chunk_collection.hpp"

namespace voxel
{
	class Chunk::Generator
	{
	private:
		Collection::CoordinateProvider::Contract _requests;

	protected:
		Collection &_collection;

		[[nodiscard]] virtual std::unique_ptr<Chunk> generate(Coordinate coordinate) const = 0;

	public:
		explicit Generator(Collection &collection);

		virtual ~Generator() = default;
	};
}
