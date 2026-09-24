#include "voxel/voxel_mesher_occlusion.hpp"

#include <algorithm>
#include <cmath>
#include <functional>
#include <optional>

#include "voxel/voxel_transform.hpp"

namespace voxel
{
	namespace
	{
		constexpr float Epsilon = 0.0001f;

		struct ProjectedVertex
		{
			spk::Vector2 position;
			spk::Vector2 uv;
		};

		using ProjectedPolygon = std::vector<ProjectedVertex>;

		float cross(spk::Vector2 left, spk::Vector2 right) noexcept
		{
			return left.x * right.y - left.y * right.x;
		}

		spk::Vector2 scaled(spk::Vector2 value, float factor) noexcept
		{
			return {value.x * factor, value.y * factor};
		}

		bool positionsAreEqual(spk::Vector2 left, spk::Vector2 right) noexcept
		{
			return std::abs(left.x - right.x) < Epsilon && std::abs(left.y - right.y) < Epsilon;
		}

		float turn(spk::Vector2 previous, spk::Vector2 current, spk::Vector2 next) noexcept
		{
			return cross(current - previous, next - current);
		}

		float signedDoubledArea(const ProjectedPolygon &polygon) noexcept
		{
			float result = 0.0f;
			for (std::size_t index = 0; index < polygon.size(); ++index)
			{
				result += cross(polygon[index].position, polygon[(index + 1) % polygon.size()].position);
			}
			return result;
		}

		ProjectedPolygon removeRedundantVertices(ProjectedPolygon polygon)
		{
			if (polygon.size() > 1 && positionsAreEqual(polygon.front().position, polygon.back().position))
			{
				polygon.pop_back();
			}
			bool changed = true;
			while (changed && polygon.size() >= 3)
			{
				changed = false;
				for (std::size_t index = 0; index < polygon.size(); ++index)
				{
					const auto &previous = polygon[(index + polygon.size() - 1) % polygon.size()];
					const auto &current = polygon[index];
					const auto &next = polygon[(index + 1) % polygon.size()];
					if (positionsAreEqual(previous.position, current.position) || std::abs(turn(previous.position, current.position, next.position)) < Epsilon)
					{
						polygon.erase(polygon.begin() + static_cast<std::ptrdiff_t>(index));
						changed = true;
						break;
					}
				}
			}
			return polygon;
		}

		bool isConvex(const ProjectedPolygon &polygon) noexcept
		{
			if (polygon.size() < 3 || std::abs(signedDoubledArea(polygon)) < Epsilon)
			{
				return false;
			}
			float previousTurn = 0.0f;
			for (std::size_t index = 0; index < polygon.size(); ++index)
			{
				const float currentTurn = turn(
					polygon[index].position,
					polygon[(index + 1) % polygon.size()].position,
					polygon[(index + 2) % polygon.size()].position);
				if (std::abs(currentTurn) < Epsilon)
				{
					continue;
				}
				if (previousTurn != 0.0f && currentTurn * previousTurn < 0.0f)
				{
					return false;
				}
				previousTurn = currentTurn;
			}
			return previousTurn != 0.0f;
		}

		void appendDistinct(ProjectedPolygon &polygon, ProjectedVertex vertex)
		{
			if (polygon.empty() || !positionsAreEqual(polygon.back().position, vertex.position))
			{
				polygon.push_back(std::move(vertex));
			}
		}

		ProjectedPolygon clipAgainstHalfPlane(
			const ProjectedPolygon &polygon,
			spk::Vector2 edgeStart,
			spk::Vector2 edgeEnd,
			float clipWinding,
			bool keepInside)
		{
			ProjectedPolygon result;
			if (polygon.empty())
			{
				return result;
			}
			const auto distance = [&](const ProjectedVertex &vertex) {
				return cross(edgeEnd - edgeStart, vertex.position - edgeStart) * clipWinding;
			};
			const auto accepted = [keepInside](float value) {
				return keepInside ? value > -Epsilon : value < Epsilon;
			};

			ProjectedVertex previous = polygon.back();
			float previousDistance = distance(previous);
			bool previousAccepted = accepted(previousDistance);
			for (const ProjectedVertex &current : polygon)
			{
				const float currentDistance = distance(current);
				const bool currentAccepted = accepted(currentDistance);
				if (currentAccepted != previousAccepted)
				{
					const float denominator = previousDistance - currentDistance;
					if (std::abs(denominator) >= Epsilon)
					{
						const float interpolation = previousDistance / denominator;
						appendDistinct(result, {.position = previous.position + scaled(current.position - previous.position, interpolation), .uv = previous.uv + scaled(current.uv - previous.uv, interpolation)});
					}
				}
				if (currentAccepted)
				{
					appendDistinct(result, current);
				}
				previous = current;
				previousDistance = currentDistance;
				previousAccepted = currentAccepted;
			}
			return removeRedundantVertices(std::move(result));
		}

		std::vector<ProjectedPolygon> subtractConvexPolygon(const ProjectedPolygon &polygon, const ProjectedPolygon &occluder)
		{
			std::vector<ProjectedPolygon> result;
			ProjectedPolygon remaining = polygon;
			const float winding = signedDoubledArea(occluder) > 0.0f ? 1.0f : -1.0f;
			for (std::size_t edge = 0; edge < occluder.size(); ++edge)
			{
				const auto start = occluder[edge].position;
				const auto end = occluder[(edge + 1) % occluder.size()].position;
				auto outside = clipAgainstHalfPlane(remaining, start, end, winding, false);
				if (outside.size() >= 3 && std::abs(signedDoubledArea(outside)) >= Epsilon)
				{
					result.push_back(std::move(outside));
				}
				remaining = clipAgainstHalfPlane(remaining, start, end, winding, true);
				if (remaining.size() < 3 || std::abs(signedDoubledArea(remaining)) < Epsilon)
				{
					break;
				}
			}
			return result;
		}

		spk::Vector2 projectPosition(spk::Vector3 position, Voxel::Side side) noexcept
		{
			switch (side)
			{
			case Voxel::Side::NegativeX:
			case Voxel::Side::PositiveX:
				return {position.y, position.z};
			case Voxel::Side::NegativeY:
			case Voxel::Side::PositiveY:
				return {position.x, position.z};
			default:
				return {position.x, position.y};
			}
		}

		spk::Vector3 restorePosition(spk::Vector2 position, Voxel::Side side, float boundaryCoordinate) noexcept
		{
			switch (side)
			{
			case Voxel::Side::NegativeX:
			case Voxel::Side::PositiveX:
				return {boundaryCoordinate, position.x, position.y};
			case Voxel::Side::NegativeY:
			case Voxel::Side::PositiveY:
				return {position.x, boundaryCoordinate, position.y};
			default:
				return {position.x, position.y, boundaryCoordinate};
			}
		}

		ProjectedPolygon projectPolygon(const Voxel::Shape::Polygon &polygon, Voxel::Side side)
		{
			ProjectedPolygon result;
			result.reserve(polygon.vertices.size());
			for (const auto &vertex : polygon.vertices)
			{
				result.push_back({projectPosition(vertex.position, side), vertex.uv});
			}
			return removeRedundantVertices(std::move(result));
		}

		float boundaryCoordinate(const Voxel::Shape::Polygon &polygon, Voxel::Side side) noexcept
		{
			const auto position = polygon.vertices.front().position;
			switch (side)
			{
			case Voxel::Side::NegativeX:
			case Voxel::Side::PositiveX:
				return position.x;
			case Voxel::Side::NegativeY:
			case Voxel::Side::PositiveY:
				return position.y;
			default:
				return position.z;
			}
		}

		Voxel::Shape::Polygon restorePolygon(
			const ProjectedPolygon &projected,
			const Voxel::Shape::Polygon &source,
			float coordinate)
		{
			Voxel::Shape::Polygon result;
			result.materialSlot = source.materialSlot;
			result.normal = source.normal;
			result.outerSide = source.outerSide;
			result.vertices.reserve(projected.size());
			for (const auto &vertex : projected)
			{
				result.vertices.push_back({restorePosition(vertex.position, source.outerSide, coordinate), vertex.uv});
			}
			return result;
		}
	}

	std::size_t VoxelMesher::OcclusionCache::KeyHash::operator()(const Key &key) const noexcept
	{
		std::size_t result = std::hash<const void *>{}(key.polygon);
		auto combine = [&result](std::size_t value) {
			result ^= value + 0x9e3779b9 + (result << 6) + (result >> 2);
		};
		combine(std::hash<const void *>{}(key.occludingShape));
		combine(key.transform);
		combine(key.occludingTransform);
		return result;
	}

	const std::vector<Voxel::Shape::Polygon> &VoxelMesher::OcclusionCache::visibleRemnants(
		const Voxel::Shape::Polygon &polygon,
		Voxel::Cell cell,
		const Voxel::Shape &occludingShape,
		Voxel::Cell occludingCell)
	{
		const std::uint32_t transformMask = Voxel::Cell::OrientationMask | Voxel::Cell::FlipMask;
		const Key key{&polygon, &occludingShape, cell.packed() & transformMask, occludingCell.packed() & transformMask};
		auto [iterator, inserted] = _visibleRemnants.try_emplace(key);
		if (inserted)
		{
			++_misses;
			iterator->second = _computeVisibleRemnants(polygon, cell, occludingShape, occludingCell);
		}
		else
		{
			++_hits;
		}
		return iterator->second;
	}

	VoxelMesher::CacheStatistics VoxelMesher::OcclusionCache::statistics() const noexcept
	{
		return {_visibleRemnants.size(), _hits, _misses};
	}

	std::vector<Voxel::Shape::Polygon> VoxelMesher::OcclusionCache::_computeVisibleRemnants(
		const Voxel::Shape::Polygon &polygon,
		Voxel::Cell cell,
		const Voxel::Shape &occludingShape,
		Voxel::Cell occludingCell)
	{
		const auto transformed = Voxel::transformPolygon(polygon, cell);
		const Voxel::Side side = transformed.outerSide;
		if (side == Voxel::Side::None)
		{
			return {transformed};
		}
		for (std::size_t sideIndex = 0; sideIndex < occludingShape.coversSide.size(); ++sideIndex)
		{
			if (occludingShape.coversSide[sideIndex] &&
				Voxel::transformSide(static_cast<Voxel::Side>(sideIndex), occludingCell) == Voxel::oppositeSide(side))
			{
				return {};
			}
		}
		ProjectedPolygon sourceProjection = projectPolygon(transformed, side);
		if (!isConvex(sourceProjection))
		{
			return {transformed};
		}

		std::vector<ProjectedPolygon> visible{std::move(sourceProjection)};
		for (const auto &occludingPolygon : occludingShape.polygons)
		{
			const auto transformedOccluder = Voxel::transformPolygon(occludingPolygon, occludingCell);
			if (transformedOccluder.outerSide != Voxel::oppositeSide(side))
			{
				continue;
			}
			const auto occluderProjection = projectPolygon(transformedOccluder, side);
			if (!isConvex(occluderProjection))
			{
				return {transformed};
			}
			std::vector<ProjectedPolygon> next;
			for (const auto &piece : visible)
			{
				auto difference = subtractConvexPolygon(piece, occluderProjection);
				next.insert(next.end(), std::make_move_iterator(difference.begin()), std::make_move_iterator(difference.end()));
			}
			visible = std::move(next);
			if (visible.empty())
			{
				break;
			}
		}

		std::vector<Voxel::Shape::Polygon> result;
		result.reserve(visible.size());
		const float coordinate = boundaryCoordinate(transformed, side);
		for (const auto &piece : visible)
		{
			result.push_back(restorePolygon(piece, transformed, coordinate));
		}
		return result;
	}
}
