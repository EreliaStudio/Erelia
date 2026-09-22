#pragma once

#include <cstddef>
#include <vector>

#include <sparkle>

namespace voxel
{
	class Palette final
	{
	public:
		using ElementIndex = std::uint32_t;

		struct Data
		{
			spk::Color color;
			bool operator==(const Data &) const noexcept = default;
		};

	private:
		static constexpr std::size_t BindingPoint = 2;
		using DataView = spk::ShaderStorageBuffer::View<void, Data>;

		spk::ShaderStorageBuffer _buffer;
		DataView _view{0, nullptr};

	public:
		Palette();

		void resize(std::size_t size);

		[[nodiscard]] Data &data(ElementIndex index);
		[[nodiscard]] const Data &data(ElementIndex index) const;
		[[nodiscard]] std::size_t size() const noexcept;
		[[nodiscard]] bool empty() const noexcept;
		[[nodiscard]] bool contains(ElementIndex elementIndex) const noexcept;
		[[nodiscard]] spk::GPUResource::Identifier resourceIdentifier() const noexcept;

		void validate();
		void activate(spk::RenderContext &context) const;
	};
}
