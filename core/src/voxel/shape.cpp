#include "erelia/core/voxel/shape.hpp"

#include <algorithm>
#include <cmath>
#include <cstdint>
#include <exception.hpp>
#include <limits>
#include <utility>

namespace
{
	struct WideVector3
	{
		std::int64_t x;
		std::int64_t y;
		std::int64_t z;
	};

	[[nodiscard]] std::int32_t vertexScale()
	{
		static const auto result = static_cast<std::int32_t>(std::round(1.0f / Voxel::Shape::VertexPrecision));
		return result;
	}

	[[nodiscard]] WideVector3 difference(const spk::Vector3Int &first, const spk::Vector3Int &second)
	{
		return {
			static_cast<std::int64_t>(first.x) - second.x,
			static_cast<std::int64_t>(first.y) - second.y,
			static_cast<std::int64_t>(first.z) - second.z};
	}

	[[nodiscard]] WideVector3 cross(const WideVector3 &first, const WideVector3 &second)
	{
		return {
			first.y * second.z - first.z * second.y,
			first.z * second.x - first.x * second.z,
			first.x * second.y - first.y * second.x};
	}

	[[nodiscard]] std::int64_t dot(const WideVector3 &first, const WideVector3 &second)
	{
		return first.x * second.x + first.y * second.y + first.z * second.z;
	}

	[[nodiscard]] bool isZero(const WideVector3 &value)
	{
		return value.x == 0 && value.y == 0 && value.z == 0;
	}

	[[nodiscard]] WideVector3 polygonNormal(const std::vector<spk::Vector3Int> &vertices)
	{
		for (std::size_t index = 2; index < vertices.size(); ++index)
		{
			const WideVector3 first = difference(vertices[1], vertices[0]);
			const WideVector3 second = difference(vertices[index], vertices[0]);
			const WideVector3 normal = cross(first, second);
			if (!isZero(normal))
			{
				return normal;
			}
		}
		return {};
	}

	[[noreturn]] void throwAt(const spk::JSON::Reader &reader, const std::string &message)
	{
		throw spk::Exception(reader.file().generic_string() + ":" + reader.path() + ": " + message);
	}

}

namespace Voxel
{
	spk::Vector3Int Shape::_loadVertex(const spk::JSON::Reader &reader)
	{
		reader.forbidUnknown({"x", "y", "z"});
		const float x = reader.require<float>("x");
		const float y = reader.require<float>("y");
		const float z = reader.require<float>("z");

		auto convert = [&](float value, const char *component) {
			if (value < 0.0f || value > 1.0f)
			{
				throw spk::Exception(
					reader.file().generic_string() + ":" + reader.pathFor(component) + ": vertex coordinate is outside [0.0, 1.0]");
			}
			return static_cast<std::int32_t>(value * static_cast<float>(vertexScale()));
		};

		return {convert(x, "x"), convert(y, "y"), convert(z, "z")};
	}

	void Shape::_validatePolygon(const Polygon &polygon, const spk::JSON::Reader &reader)
	{
		if (polygon.vertices.size() < 3)
		{
			throwAt(reader, "voxel polygon needs at least three vertices");
		}

		for (std::size_t index = 0; index < polygon.vertices.size(); ++index)
		{
			if (polygon.vertices[index] == polygon.vertices[(index + 1) % polygon.vertices.size()])
			{
				throwAt(reader, "voxel polygon has duplicate adjacent vertices");
			}
		}

		const WideVector3 normal = polygonNormal(polygon.vertices);
		if (isZero(normal))
		{
			throwAt(reader, "voxel polygon is degenerate");
		}

		for (const spk::Vector3Int &vertex : polygon.vertices)
		{
			if (dot(normal, difference(vertex, polygon.vertices[0])) != 0)
			{
				throwAt(reader, "voxel polygon is not planar");
			}
		}

		for (std::size_t edgeIndex = 0; edgeIndex < polygon.vertices.size(); ++edgeIndex)
		{
			const spk::Vector3Int &edgeStart = polygon.vertices[edgeIndex];
			const spk::Vector3Int &edgeEnd = polygon.vertices[(edgeIndex + 1) % polygon.vertices.size()];
			const WideVector3 edge = difference(edgeEnd, edgeStart);

			for (std::size_t vertexIndex = 0; vertexIndex < polygon.vertices.size(); ++vertexIndex)
			{
				if (vertexIndex == edgeIndex || vertexIndex == (edgeIndex + 1) % polygon.vertices.size())
				{
					continue;
				}

				const WideVector3 towardVertex = difference(polygon.vertices[vertexIndex], edgeStart);
				if (dot(cross(edge, towardVertex), normal) < 0)
				{
					throwAt(reader, "voxel polygon is concave or self-intersecting");
				}
			}
		}
	}

	spk::Vector3 Shape::_normalOf(const std::vector<spk::Vector3Int> &vertices)
	{
		const WideVector3 normal = polygonNormal(vertices);
		return spk::Vector3(static_cast<float>(normal.x), static_cast<float>(normal.y), static_cast<float>(normal.z)).normalized();
	}

	Shape::Polygon Shape::_loadPolygon(const spk::JSON::Reader &reader)
	{
		reader.forbidUnknown({"slot", "vertices"});

		Polygon result;
		result.slot = reader.require<std::string>("slot");
		if (result.slot.empty())
		{
			throw spk::Exception(reader.file().generic_string() + ":" + reader.pathFor("slot") + ": voxel polygon slot cannot be empty");
		}

		for (const spk::JSON::Reader &vertexReader : reader.childArray("vertices"))
		{
			result.vertices.push_back(_loadVertex(vertexReader));
		}

		_validatePolygon(result, reader);
		result.normal = _normalOf(result.vertices);
		return result;
	}

	Shape::Polygon Shape::_transformPolygon(
		const Polygon &polygon,
		Cell::Orientation orientation,
		Cell::FlipOrientation flipOrientation)
	{
		const std::int32_t maximum = vertexScale();
		Polygon result;
		result.slot = polygon.slot;
		result.vertices.reserve(polygon.vertices.size());

		for (const spk::Vector3Int &source : polygon.vertices)
		{
			spk::Vector3Int transformed = source;
			switch (orientation)
			{
			case Cell::Orientation::PositiveX:
				break;
			case Cell::Orientation::NegativeZ:
				transformed.x = source.z;
				transformed.z = maximum - source.x;
				break;
			case Cell::Orientation::NegativeX:
				transformed.x = maximum - source.x;
				transformed.z = maximum - source.z;
				break;
			case Cell::Orientation::PositiveZ:
				transformed.x = maximum - source.z;
				transformed.z = source.x;
				break;
			}

			if (flipOrientation == Cell::FlipOrientation::NegativeY)
			{
				transformed.y = maximum - source.y;
			}
			result.vertices.push_back(transformed);
		}

		if (flipOrientation == Cell::FlipOrientation::NegativeY)
		{
			std::ranges::reverse(result.vertices);
		}

		result.normal = _normalOf(result.vertices);
		return result;
	}

	Shape::Shape()
	{
		_orientedPolygons[0].uuid.store(spk::UUID::generate(), std::memory_order_release);
	}

	Shape::Shape(Shape &&other) noexcept
	{
		for (std::size_t index = 0; index < _orientedPolygons.size(); ++index)
		{
			OrientedPolygonArray &source = other._orientedPolygons[index];
			OrientedPolygonArray &destination = _orientedPolygons[index];

			destination.polygons = std::move(source.polygons);
			destination.uuid.store(source.uuid.load(std::memory_order_relaxed), std::memory_order_relaxed);
			source.uuid.store(spk::UUID::null(), std::memory_order_relaxed);
		}
	}

	Shape::Shape(const spk::JSON::Reader &reader)
	{
		reader.forbidUnknown({"polygons"});
		auto &canonical = _orientedPolygons[0];
		for (const spk::JSON::Reader &polygonReader : reader.childArray("polygons"))
		{
			canonical.polygons.push_back(_loadPolygon(polygonReader));
		}
		if (canonical.polygons.empty())
		{
			throwAt(reader, "voxel shape has no polygons");
		}
		canonical.uuid.store(spk::UUID::generate(), std::memory_order_release);
	}

	const std::vector<Shape::Polygon> &Shape::polygons() const noexcept
	{
		return _orientedPolygons[0].polygons;
	}

	const Shape::OrientedPolygonArray &Shape::orientedPolygons(
		Cell::Orientation orientation,
		Cell::FlipOrientation flipOrientation) const
	{
		const auto orientationValue = static_cast<std::uint8_t>(orientation);
		const auto flipValue = static_cast<std::uint8_t>(flipOrientation);
		if (orientationValue > 3u)
		{
			throw spk::Exception("Voxel::Shape Orientation is invalid");
		}
		if (flipValue > 1u)
		{
			throw spk::Exception("Voxel::Shape FlipOrientation is invalid");
		}

		const std::size_t index = static_cast<std::size_t>(orientationValue) + 4u * static_cast<std::size_t>(flipValue);
		OrientedPolygonArray &entry = _orientedPolygons[index];
		if (!entry.uuid.load(std::memory_order_acquire).isNull())
		{
			return entry;
		}

		std::scoped_lock lock(_orientedPolygonMutex);
		if (!entry.uuid.load(std::memory_order_acquire).isNull())
		{
			return entry;
		}

		std::vector<Polygon> transformed;
		transformed.reserve(_orientedPolygons[0].polygons.size());
		for (const Polygon &polygon : _orientedPolygons[0].polygons)
		{
			transformed.push_back(_transformPolygon(polygon, orientation, flipOrientation));
		}

		entry.polygons = std::move(transformed);
		entry.uuid.store(spk::UUID::generate(), std::memory_order_release);
		return entry;
	}
}
