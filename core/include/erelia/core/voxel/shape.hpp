#pragma once

#include <array>
#include <atomic>
#include <mutex>
#include <string>
#include <type_traits>
#include <vector>

#include <container/json/reader.hpp>
#include <math/vector3.hpp>
#include <type/uuid.hpp>

#include "erelia/core/voxel/cell.hpp"

namespace Voxel
{
	struct Shape
	{
		using ID = std::string;
		static constexpr float VertexPrecision = 0.001f;

		struct Polygon
		{
			std::vector<spk::Vector3Int> vertices;
			std::string slot;
			spk::Vector3 normal;
		};

		struct OrientedPolygonArray
		{
			std::atomic<spk::UUID> uuid{spk::UUID::null()};
			std::vector<Polygon> polygons;
		};

		class Catalog;

	private:
		mutable std::array<OrientedPolygonArray, 8> _orientedPolygons;
		mutable std::mutex _orientedPolygonMutex;

		Shape();
		explicit Shape(const spk::JSON::Reader &reader);

		[[nodiscard]] static Polygon _loadPolygon(const spk::JSON::Reader &reader);
		[[nodiscard]] static spk::Vector3Int _loadVertex(const spk::JSON::Reader &reader);
		[[nodiscard]] static Polygon _transformPolygon(
			const Polygon &polygon,
			Cell::Orientation orientation,
			Cell::FlipOrientation flipOrientation);
		[[nodiscard]] static spk::Vector3 _normalOf(const std::vector<spk::Vector3Int> &vertices);
		static void _validatePolygon(const Polygon &polygon, const spk::JSON::Reader &reader);

		friend class Catalog;

	public:
		Shape(const Shape &) = delete;
		Shape &operator=(const Shape &) = delete;
		Shape(Shape &&other) noexcept;
		Shape &operator=(Shape &&) = delete;
		~Shape() = default;

		[[nodiscard]] const std::vector<Polygon> &polygons() const noexcept;
		[[nodiscard]] const OrientedPolygonArray &orientedPolygons(
			Cell::Orientation orientation,
			Cell::FlipOrientation flipOrientation) const;
	};

	static_assert(std::is_trivially_copyable_v<spk::UUID>);
	static_assert(std::is_same_v<std::atomic<spk::UUID>::value_type, spk::UUID>);
}
