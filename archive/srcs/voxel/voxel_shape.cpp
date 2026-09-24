#include "voxel/voxel_shape.hpp"

#include <algorithm>
#include <cmath>
#include <exception.hpp>

namespace voxel::Voxel
{
	bool Shape::_isNormalized(const spk::Vector3 &position)
	{
		return position.x >= -Epsilon && position.x <= 1.0f + Epsilon &&
			   position.y >= -Epsilon && position.y <= 1.0f + Epsilon &&
			   position.z >= -Epsilon && position.z <= 1.0f + Epsilon;
	}

	spk::Vector3Int Shape::_sideOffset(Side side)
	{
		switch (side)
		{
		case Side::NegativeX: return {-1, 0, 0};
		case Side::PositiveX: return {1, 0, 0};
		case Side::NegativeY: return {0, -1, 0};
		case Side::PositiveY: return {0, 1, 0};
		case Side::NegativeZ: return {0, 0, -1};
		case Side::PositiveZ: return {0, 0, 1};
		default: return {};
		}
	}

	Side Shape::_boundarySide(const std::vector<Vertex> &vertices)
	{
		auto allAt = [&](auto coordinate, float value) {
			return std::ranges::all_of(vertices, [&](const Vertex &vertex) {
				return std::abs(coordinate(vertex.position) - value) < Epsilon;
			});
		};

		if (allAt([](const auto &p) { return p.x; }, 0.0f)) return Side::NegativeX;
		if (allAt([](const auto &p) { return p.x; }, 1.0f)) return Side::PositiveX;
		if (allAt([](const auto &p) { return p.y; }, 0.0f)) return Side::NegativeY;
		if (allAt([](const auto &p) { return p.y; }, 1.0f)) return Side::PositiveY;
		if (allAt([](const auto &p) { return p.z; }, 0.0f)) return Side::NegativeZ;
		if (allAt([](const auto &p) { return p.z; }, 1.0f)) return Side::PositiveZ;
		return Side::None;
	}

	std::pair<float, float> Shape::_boundaryCoordinates(const spk::Vector3 &position, Side side)
	{
		if (side == Side::NegativeX || side == Side::PositiveX)
			return {position.y, position.z};
		if (side == Side::NegativeY || side == Side::PositiveY)
			return {position.x, position.z};
		return {position.x, position.y};
	}

	bool Shape::_fullBoundary(const std::vector<Vertex> &vertices, Side side)
	{
		if (vertices.size() != 4 || side == Side::None)
			return false;

		float minA = 1.0f, minB = 1.0f, maxA = 0.0f, maxB = 0.0f;
		for (const Vertex &vertex : vertices)
		{
			const auto [a, b] = _boundaryCoordinates(vertex.position, side);
			minA = std::min(minA, a);
			minB = std::min(minB, b);
			maxA = std::max(maxA, a);
			maxB = std::max(maxB, b);
		}
		return minA < Epsilon && minB < Epsilon && maxA > 1.0f - Epsilon && maxB > 1.0f - Epsilon;
	}

	spk::Vector3 Shape::_normalOf(const std::vector<Vertex> &vertices)
	{
		if (vertices.size() < 3)
			throw spk::Exception("voxel polygon needs at least three vertices");

		const auto firstEdge = vertices[1].position - vertices[0].position;
		const auto secondEdge = vertices[2].position - vertices[0].position;
		return firstEdge.cross(secondEdge).normalized();
	}

	Shape::Vertex Shape::_loadVertex(const spk::JSON::Reader &reader, const std::string &shapeName)
	{
		reader.forbidUnknown({"position", "uv"});
		Vertex result{
			spk::Vector3(reader.value().at("position")),
			spk::Vector2(reader.value().at("uv"))};

		if (!_isNormalized(result.position))
			throw spk::Exception(reader.file().generic_string() + ":" + reader.path() + ": shape '" + shapeName + "' has a vertex outside normalized cell space");
		return result;
	}

	void Shape::_orientPolygon(Polygon &polygon)
	{
		polygon.normal = _normalOf(polygon.vertices);
		polygon.outerSide = _boundarySide(polygon.vertices);
		if (polygon.outerSide == Side::None)
			return;

		const auto outward = spk::Vector3(_sideOffset(polygon.outerSide));
		if (polygon.normal.dot(outward) < 0.0f)
		{
			std::ranges::reverse(polygon.vertices);
			polygon.normal = _normalOf(polygon.vertices);
		}
	}

	Shape::Polygon Shape::_loadPolygon(const spk::JSON::Reader &reader, const std::string &shapeName)
	{
		reader.forbidUnknown({"slot", "vertices"});
		Polygon result;
		result.materialSlot = reader.require<std::string>("slot");

		for (const spk::JSON::Reader &vertexReader : reader.childArray("vertices"))
			result.vertices.push_back(_loadVertex(vertexReader, shapeName));

		_orientPolygon(result);
		result.fullCoverage = _fullBoundary(result.vertices, result.outerSide);
		return result;
	}

	void Shape::_appendPolygon(Polygon polygon)
	{
		materialSlots.insert(polygon.materialSlot);
		if (polygon.fullCoverage)
			coversSide[static_cast<std::size_t>(polygon.outerSide)] = true;
		polygons.push_back(std::move(polygon));
	}

	void Shape::_loadPolygons(const spk::JSON::Reader &reader)
	{
		for (const spk::JSON::Reader &polygonReader : reader.childArray("polygons"))
			_appendPolygon(_loadPolygon(polygonReader, name));

		if (polygons.empty())
			throw spk::Exception(reader.file().generic_string() + ":" + reader.path() + ": shape '" + name + "' has no polygons");
	}

	Shape::Shape(const spk::JSON::Reader &reader)
	{
		reader.forbidUnknown({"id", "polygons"});
		name = reader.require<std::string>("id");
		if (name.empty())
			throw spk::Exception(reader.file().generic_string() + ":" + reader.pathFor("id") + ": voxel shape id cannot be empty");
		_loadPolygons(reader);
	}
}
