#include "erelia/core/voxel/catalog.hpp"

#include <container/json/error.hpp>
#include <container/json/reader.hpp>

namespace Voxel
{
	Shape::ID Shape::Catalog::_parseKey(const spk::JSON::Reader &reader) const
	{
		const Shape::ID id = reader.require<Shape::ID>("id");
		if (id.empty())
		{
			spk::JSON::throwAt(reader.file(), reader.pathFor("id"), "voxel Shape ID cannot be empty");
		}
		return id;
	}

	Shape Shape::Catalog::_parseElement(const spk::JSON::Reader &reader) const
	{
		return Shape(reader);
	}
}
