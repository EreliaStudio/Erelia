#include "voxel/voxel_transform.hpp"

namespace voxel::Voxel
{
	Side oppositeSide(Side side) noexcept
	{
		switch (side)
		{
		case Side::NegativeX:
			return Side::PositiveX;
		case Side::PositiveX:
			return Side::NegativeX;
		case Side::NegativeY:
			return Side::PositiveY;
		case Side::PositiveY:
			return Side::NegativeY;
		case Side::NegativeZ:
			return Side::PositiveZ;
		case Side::PositiveZ:
			return Side::NegativeZ;
		default:
			return Side::None;
		}
	}

	spk::Vector3Int sideOffset(Side side) noexcept
	{
		switch (side)
		{
		case Side::NegativeX:
			return {-1, 0, 0};
		case Side::PositiveX:
			return {1, 0, 0};
		case Side::NegativeY:
			return {0, -1, 0};
		case Side::PositiveY:
			return {0, 1, 0};
		case Side::NegativeZ:
			return {0, 0, -1};
		case Side::PositiveZ:
			return {0, 0, 1};
		default:
			return {};
		}
	}

	spk::Vector3 transformPosition(spk::Vector3 position, Cell cell) noexcept
	{
		if (cell.flip() == Flip::NegativeY)
		{
			position.y = 1.0f - position.y;
		}
		const float x = position.x - .5f;
		const float z = position.z - .5f;
		switch (cell.orientation())
		{
		case Orientation::PositiveX:
			position.x = .5f + z;
			position.z = .5f - x;
			break;
		case Orientation::NegativeZ:
			position.x = .5f - x;
			position.z = .5f - z;
			break;
		case Orientation::NegativeX:
			position.x = .5f - z;
			position.z = .5f + x;
			break;
		default:
			break;
		}
		return position;
	}

	spk::Vector3 transformNormal(spk::Vector3 normal, Cell cell) noexcept
	{
		if (cell.flip() == Flip::NegativeY)
		{
			normal.y = -normal.y;
		}
		const float x = normal.x;
		const float z = normal.z;
		switch (cell.orientation())
		{
		case Orientation::PositiveX:
			normal.x = z;
			normal.z = -x;
			break;
		case Orientation::NegativeZ:
			normal.x = -x;
			normal.z = -z;
			break;
		case Orientation::NegativeX:
			normal.x = -z;
			normal.z = x;
			break;
		default:
			break;
		}
		return normal;
	}

	Side transformSide(Side side, Cell cell) noexcept
	{
		if (side == Side::None)
		{
			return side;
		}
		spk::Vector3 normal{};
		switch (side)
		{
		case Side::NegativeX:
			normal.x = -1;
			break;
		case Side::PositiveX:
			normal.x = 1;
			break;
		case Side::NegativeY:
			normal.y = -1;
			break;
		case Side::PositiveY:
			normal.y = 1;
			break;
		case Side::NegativeZ:
			normal.z = -1;
			break;
		case Side::PositiveZ:
			normal.z = 1;
			break;
		default:
			break;
		}
		normal = transformNormal(normal, cell);
		if (normal.x < -.5f)
		{
			return Side::NegativeX;
		}
		if (normal.x > .5f)
		{
			return Side::PositiveX;
		}
		if (normal.y < -.5f)
		{
			return Side::NegativeY;
		}
		if (normal.y > .5f)
		{
			return Side::PositiveY;
		}
		if (normal.z < -.5f)
		{
			return Side::NegativeZ;
		}
		return Side::PositiveZ;
	}

	Shape::Polygon transformPolygon(const Shape::Polygon &polygon, Cell cell)
	{
		Shape::Polygon result = polygon;
		for (auto &vertex : result.vertices)
		{
			vertex.position = transformPosition(vertex.position, cell);
		}
		result.normal = transformNormal(result.normal, cell);
		result.outerSide = transformSide(result.outerSide, cell);
		return result;
	}
}
