#pragma once

#include "voxel/voxel_volume.hpp"

namespace voxel
{
	class VoxelVolume::Editor final
	{
	private:
		VoxelVolume *_volume;
		bool _changed = false;

		friend class VoxelVolume;

		explicit Editor(VoxelVolume &volume) :
			_volume(&volume)
		{
		}

	public:
		Editor(Editor &&other) noexcept;
		Editor(const Editor &) = delete;

		~Editor();

		Editor &operator=(const Editor &) = delete;

		bool set(spk::Vector3Int coordinate, Voxel::Cell value);
		void commit();
	};
}
