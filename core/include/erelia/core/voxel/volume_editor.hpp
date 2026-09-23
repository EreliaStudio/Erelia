#pragma once

#include "erelia/core/voxel/volume.hpp"

namespace Voxel
{
	class Volume::Editor final
	{
	private:
		Volume *_volume;
		bool _changed = false;

		friend class Volume;

		explicit Editor(Volume &volume) noexcept;

	public:
		Editor(Editor &&other) noexcept;
		Editor(const Editor &) = delete;
		~Editor();

		Editor &operator=(const Editor &) = delete;
		Editor &operator=(Editor &&) = delete;

		bool set(const LocalCoordinate &coordinate, Cell value);
		void commit();
	};
}
