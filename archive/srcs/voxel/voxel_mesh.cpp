#include "voxel/voxel_mesh.hpp"

namespace voxel
{
	void VoxelMesh::Builder::_setupAttributes(spk::VertexBuffer &vertexBuffer)
	{
		vertexBuffer.addAttribute(0, spk::VertexBuffer::Attribute::Type::Float, 3);
		vertexBuffer.addAttribute(1, spk::VertexBuffer::Attribute::Type::Float, 3);
		vertexBuffer.addAttribute(2, spk::VertexBuffer::Attribute::Type::UnsignedInt, 1,
			spk::VertexBuffer::Interpretation::Integer);
	}
}
