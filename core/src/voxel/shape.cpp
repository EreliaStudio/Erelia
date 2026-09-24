#include "erelia/core/voxel/shape.hpp"

#include <algorithm>
#include <cmath>
#include <cstdint>
#include <exception>
#include <utility>

#include <container/json/error.hpp>
#include <exception.hpp>

namespace
{
	using WideVertex = spk::TVector3<std::int64_t>;

	[[nodiscard]] std::int32_t vertexScale()
	{
		static const auto result = static_cast<std::int32_t>(std::round(1.0f / Voxel::Shape::VertexPrecision));
		return result;
	}

	[[nodiscard]] std::int32_t quantizeVertexComponent(
		const spk::JSON::Reader &reader,
		float value,
		std::size_t componentIndex)
	{
		if (value < 0.0f || value > 1.0f)
		{
			spk::JSON::throwAt(
				reader.file(),
				reader.path() + "[" + std::to_string(componentIndex) + "]",
				"vertex coordinate is outside [0.0, 1.0]");
		}
		return static_cast<std::int32_t>(value * static_cast<float>(vertexScale()));
	}

	[[nodiscard]] WideVertex difference(const Voxel::Vertex &first, const Voxel::Vertex &second)
	{
		return {
			static_cast<std::int64_t>(first.x) - second.x,
			static_cast<std::int64_t>(first.y) - second.y,
			static_cast<std::int64_t>(first.z) - second.z};
	}

	[[nodiscard]] bool isZero(const WideVertex &value)
	{
		return value.x == 0 && value.y == 0 && value.z == 0;
	}

	[[nodiscard]] WideVertex polygonNormal(const std::vector<Voxel::Vertex> &vertices)
	{
		for (std::size_t index = 2; index < vertices.size(); ++index)
		{
			const WideVertex first = difference(vertices[1], vertices[0]);
			const WideVertex second = difference(vertices[index], vertices[0]);
			const WideVertex normal = first.cross(second);
			if (!isZero(normal))
			{
				return normal;
			}
		}
		return {};
	}
}

namespace Voxel
{
	Vertex Shape::_loadVertex(const spk::JSON::Reader &reader)
	{
		spk::Vector3 position;
		try
		{
			position = spk::Vector3::fromJSON(reader.value());
		} catch (...)
		{
			spk::JSON::throwAt(
				reader.file(),
				reader.path(),
				"invalid voxel vertex",
				std::current_exception());
		}

		return {
			quantizeVertexComponent(reader, position.x, 0u),
			quantizeVertexComponent(reader, position.y, 1u),
			quantizeVertexComponent(reader, position.z, 2u)};
	}

	void Shape::_validatePolygon(const Polygon &polygon, const spk::JSON::Reader &reader)
	{
		if (polygon.vertices.size() < 3)
		{
			spk::JSON::throwAt(reader.file(), reader.path(), "voxel polygon needs at least three vertices");
		}

		for (std::size_t index = 0; index < polygon.vertices.size(); ++index)
		{
			if (polygon.vertices[index] == polygon.vertices[(index + 1) % polygon.vertices.size()])
			{
				spk::JSON::throwAt(reader.file(), reader.path(), "voxel polygon has duplicate adjacent vertices");
			}
		}

		const WideVertex normal = polygonNormal(polygon.vertices);
		if (isZero(normal))
		{
			spk::JSON::throwAt(reader.file(), reader.path(), "voxel polygon is degenerate");
		}

		for (const Vertex &vertex : polygon.vertices)
		{
			if (normal.dot(difference(vertex, polygon.vertices[0])) != 0)
			{
				spk::JSON::throwAt(reader.file(), reader.path(), "voxel polygon is not planar");
			}
		}

		for (std::size_t edgeIndex = 0; edgeIndex < polygon.vertices.size(); ++edgeIndex)
		{
			const Vertex &edgeStart = polygon.vertices[edgeIndex];
			const Vertex &edgeEnd = polygon.vertices[(edgeIndex + 1) % polygon.vertices.size()];
			const WideVertex edge = difference(edgeEnd, edgeStart);

			for (std::size_t vertexIndex = 0; vertexIndex < polygon.vertices.size(); ++vertexIndex)
			{
				if (vertexIndex == edgeIndex || vertexIndex == (edgeIndex + 1) % polygon.vertices.size())
				{
					continue;
				}

				const WideVertex towardVertex = difference(polygon.vertices[vertexIndex], edgeStart);
				if (edge.cross(towardVertex).dot(normal) < 0)
				{
					spk::JSON::throwAt(reader.file(), reader.path(), "voxel polygon is concave or self-intersecting");
				}
			}
		}
	}

	spk::Vector3 Shape::_normalOf(const std::vector<Vertex> &vertices)
	{
		const WideVertex normal = polygonNormal(vertices);
		return spk::Vector3(static_cast<float>(normal.x), static_cast<float>(normal.y), static_cast<float>(normal.z)).normalized();
	}

	Shape::Polygon Shape::_loadPolygon(const spk::JSON::Reader &reader)
	{
		reader.forbidUnknown({"slot", "vertices"});

		Polygon result;
		result.slot = reader.require<Material::SlotID>("slot");
		if (result.slot.empty())
		{
			spk::JSON::throwAt(reader.file(), reader.pathFor("slot"), "voxel polygon slot cannot be empty");
		}

		if (!reader.contains("vertices"))
		{
			spk::JSON::throwAt(reader.file(), reader.pathFor("vertices"), "missing required field");
		}

		const spk::JSON::Value &verticesValue = reader.value().at("vertices");
		if (!verticesValue.isArray())
		{
			spk::JSON::throwAt(reader.file(), reader.pathFor("vertices"), "expected an array");
		}

		const spk::JSON::Value::Array &vertices = verticesValue.asArray();
		result.vertices.reserve(vertices.size());
		for (std::size_t index = 0; index < vertices.size(); ++index)
		{
			const spk::JSON::Reader vertexReader(
				vertices[index],
				reader.file(),
				reader.pathFor("vertices") + "[" + std::to_string(index) + "]");
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

		for (const Vertex &source : polygon.vertices)
		{
			Vertex transformed = source;
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
			spk::JSON::throwAt(reader.file(), reader.path(), "voxel shape has no polygons");
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
		const std::size_t index =
			static_cast<std::size_t>(orientation) +
			4u * static_cast<std::size_t>(flipOrientation);
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
