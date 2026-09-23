#include "erelia/core/voxel/definition.hpp"

#include <utility>

namespace Voxel
{
	Definition::Definition(std::shared_ptr<const Shape> shape, SlotBindings slots) :
		_shape(std::move(shape)),
		_slots(std::move(slots))
	{
	}

	const std::shared_ptr<const Shape> &Definition::shape() const noexcept
	{
		return _shape;
	}

	const Definition::SlotBindings &Definition::slots() const noexcept
	{
		return _slots;
	}
}
