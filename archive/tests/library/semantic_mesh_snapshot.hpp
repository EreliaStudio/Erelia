#pragma once

#include <string>
#include <string_view>

#include <sparkle>

namespace playground_test
{
	[[nodiscard]] std::string semanticMeshSnapshot(const spk::TextureMesh3D &mesh);
	[[nodiscard]] std::string loadCurrentBakerSemanticSnapshot(std::string_view name);
}
