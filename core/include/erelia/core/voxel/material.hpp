#pragma once

#include <string>

namespace Voxel
{
	struct Material
	{
		using ID = std::string;
		using SlotID = std::string;

		inline static const ID InvalidID = "InvalidID";
	};
}
