#include "voxel/palette.hpp"

#include <type_traits>

#include <exception.hpp>

namespace voxel
{
	static_assert(std::is_trivially_copyable_v<Palette::Data>);
	static_assert(sizeof(Palette::Data) == sizeof(float) * 4);

	Palette::Palette() :
		_buffer(BindingPoint, 0, sizeof(Data))
	{
	}

	void Palette::resize(std::size_t size)
	{
		_buffer.resize(size);
		_view = size == 0
			? DataView{0, nullptr}
			: _buffer.cast<void, Data>();
	}

	Palette::Data &Palette::data(ElementIndex index)
	{
		if (!contains(index)) throw spk::Exception("Palette data index is out of range");
		return _view.dynamicArray[index];
	}

	const Palette::Data &Palette::data(ElementIndex index) const
	{
		if (!contains(index)) throw spk::Exception("Palette data index is out of range");
		return _view.dynamicArray[index];
	}

	std::size_t Palette::size() const noexcept
	{
		return _view.nbElement;
	}

	bool Palette::empty() const noexcept
	{
		return size() == 0;
	}

	bool Palette::contains(ElementIndex elementIndex) const noexcept
	{
		return elementIndex < size();
	}

	spk::GPUResource::Identifier Palette::resourceIdentifier() const noexcept
	{
		return _buffer.identifier();
	}

	void Palette::validate()
	{
		_buffer.validate();
	}

	void Palette::activate(spk::RenderContext &context) const
	{
		_buffer.activate(context);
	}
}
