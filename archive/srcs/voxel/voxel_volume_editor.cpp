#include "voxel/voxel_volume_editor.hpp"

#include <exception.hpp>
#include <utility>

namespace voxel
{
	VoxelVolume::Editor::Editor(Editor &&other) noexcept :
		_volume(std::exchange(other._volume, nullptr)),
		_changed(other._changed)
	{
	}

	VoxelVolume::Editor::~Editor()
	{
		commit();
	}

	bool VoxelVolume::Editor::set(spk::Vector3Int coordinate, Voxel::Cell value)
	{
		if (!_volume)
			throw spk::Exception("voxel volume editor was committed");
		auto &cell = _volume->_cells[_volume->_index(coordinate)];
		if (cell == value)
			return false;
		cell = value;
		_changed = true;
		return true;
	}

	void VoxelVolume::Editor::commit()
	{
		if (!_volume)
			return;
		auto *volume = std::exchange(_volume, nullptr);
		if (_changed)
			volume->invalidate();
	}
}
