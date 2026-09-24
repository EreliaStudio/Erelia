#pragma once

#include <cstdint>
#include <map>

#include "erelia/core/voxel/material.hpp"

namespace Voxel
{
	struct Shape;

	struct Definition
	{
		using ID = std::uint32_t;
		using SlotBindings = std::map<Material::SlotID, Material::ID>;

		class Catalog;

	private:
		const Shape &_shape;
		SlotBindings _slots;

		Definition(const Shape &shape, SlotBindings slots);

		friend class Catalog;

	public:
		Definition(const Definition &) = default;
		Definition &operator=(const Definition &) = delete;
		Definition(Definition &&) noexcept = default;
		Definition &operator=(Definition &&) noexcept = delete;
		~Definition() = default;

		[[nodiscard]] const Shape &shape() const noexcept;
		[[nodiscard]] const SlotBindings &slots() const noexcept;
	};
}
