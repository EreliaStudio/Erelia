#pragma once

#include <cstdint>
#include <map>
#include <memory>
#include <string>

#include "erelia/core/voxel/material.hpp"

namespace Voxel
{
	struct Shape;

	struct Definition
	{
		using ID = std::uint32_t;
		using SlotBindings = std::map<std::string, Material::ID>;

		class Catalog;

	private:
		std::shared_ptr<const Shape> _shape;
		SlotBindings _slots;

		Definition() = default;
		Definition(std::shared_ptr<const Shape> shape, SlotBindings slots);

		friend class Catalog;

	public:
		Definition(const Definition &) = default;
		Definition &operator=(const Definition &) = default;
		Definition(Definition &&) noexcept = default;
		Definition &operator=(Definition &&) noexcept = default;
		~Definition() = default;

		[[nodiscard]] const std::shared_ptr<const Shape> &shape() const noexcept;
		[[nodiscard]] const SlotBindings &slots() const noexcept;
	};
}
