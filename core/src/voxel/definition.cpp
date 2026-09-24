#include "erelia/core/voxel/definition.hpp"

#include <utility>

namespace Voxel
{
	Definition::Definition(const Shape &shape, SlotBindings slots) :
		_shape(shape),
		_slots(std::move(slots))
	{
	}

	const Shape &Definition::shape() const noexcept
	{
		return _shape;
	}

	const Definition::SlotBindings &Definition::slots() const noexcept
	{
		return _slots;
	}
}
