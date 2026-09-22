#include "semantic_mesh_snapshot.hpp"

#include <algorithm>
#include <array>
#include <cmath>
#include <filesystem>
#include <fstream>
#include <iterator>
#include <sstream>
#include <vector>

#include <exception.hpp>

namespace playground_test
{
	namespace
	{
		using SemanticVertex = std::array<long long, 8>;
		using SemanticTriangle = std::array<SemanticVertex, 3>;

		long long quantize(float value) noexcept
		{
			return std::llround(static_cast<double>(value) * 1'000'000.0);
		}

		SemanticVertex semanticVertex(const spk::Texture3DVertex &vertex) noexcept
		{
			return {quantize(vertex.position.x), quantize(vertex.position.y), quantize(vertex.position.z),
				quantize(vertex.normal.x), quantize(vertex.normal.y), quantize(vertex.normal.z),
				quantize(vertex.uv.x), quantize(vertex.uv.y)};
		}

		SemanticTriangle canonicalTriangle(SemanticVertex a, SemanticVertex b, SemanticVertex c) noexcept
		{
			return std::min({SemanticTriangle{a, b, c}, SemanticTriangle{b, c, a}, SemanticTriangle{c, a, b}});
		}

		void appendVertex(std::ostringstream &stream, const SemanticVertex &vertex)
		{
			stream << vertex[0] << ',' << vertex[1] << ',' << vertex[2] << '|'
				   << vertex[3] << ',' << vertex[4] << ',' << vertex[5] << '|'
				   << vertex[6] << ',' << vertex[7];
		}

		void appendTriangle(std::ostringstream &stream, const SemanticTriangle &triangle)
		{
			stream << "t ";
			for (std::size_t index = 0; index < triangle.size(); ++index)
			{
				if (index != 0) stream << ';';
				appendVertex(stream, triangle[index]);
			}
			stream << '\n';
		}
	}

	std::string semanticMeshSnapshot(const spk::TextureMesh3D &mesh)
	{
		const auto vertices = mesh.layout().vertexBuffer().cast<spk::Texture3DVertex>();
		const auto indices = mesh.layout().indexBuffer().cast<spk::TextureMesh3D::Index>();
		if (indices.size() % 3 != 0) throw spk::Exception("semantic mesh snapshot requires triangles");
		std::vector<SemanticTriangle> triangles;
		triangles.reserve(indices.size() / 3);
		for (std::size_t index = 0; index < indices.size(); index += 3)
		{
			triangles.push_back(canonicalTriangle(semanticVertex(vertices[indices[index]]),
				semanticVertex(vertices[indices[index + 1]]), semanticVertex(vertices[indices[index + 2]])));
		}
		std::ranges::sort(triangles);
		std::ostringstream result;
		result << "semantic-mesh-v1\ntriangles=" << triangles.size() << '\n';
		for (const auto &triangle : triangles) appendTriangle(result, triangle);
		return result.str();
	}

	std::string loadCurrentBakerSemanticSnapshot(std::string_view name)
	{
		const auto path = std::filesystem::path(PLAYGROUND_TEST_RESOURCE_DIR) /
			"current_baker_semantics" / (std::string(name) + ".mesh");
		std::ifstream stream(path, std::ios::binary);
		if (!stream) throw spk::Exception("cannot open semantic mesh snapshot '" + path.generic_string() + "'");
		std::string result{std::istreambuf_iterator<char>(stream), std::istreambuf_iterator<char>()};
		std::erase(result, '\r');
		return result;
	}
}
