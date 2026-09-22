#pragma once

#include <array>
#include <set>
#include <string>
#include <utility>
#include <vector>

#include <sparkle>

#include "voxel/voxel.hpp"

namespace voxel::Voxel
{
	struct Shape
	{
	public:
		struct Vertex
		{
			spk::Vector3 position;
			spk::Vector2 uv;
		};

		struct Polygon
		{
			std::vector<Vertex> vertices;
			std::string materialSlot;
			spk::Vector3 normal;
			Side outerSide = Side::None;
			bool fullCoverage = false;
		};

	private:
		static constexpr float Epsilon = 0.0001f;

		[[nodiscard]] static bool _isNormalized(const spk::Vector3 &position);
		[[nodiscard]] static spk::Vector3Int _sideOffset(Side side);
		[[nodiscard]] static Side _boundarySide(const std::vector<Vertex> &vertices);
		[[nodiscard]] static bool _fullBoundary(const std::vector<Vertex> &vertices, Side side);
		[[nodiscard]] static spk::Vector3 _normalOf(const std::vector<Vertex> &vertices);
		[[nodiscard]] static std::pair<float, float> _boundaryCoordinates(const spk::Vector3 &position, Side side);
		[[nodiscard]] static Vertex _loadVertex(const spk::JSON::Reader &reader, const std::string &shapeName);
		[[nodiscard]] static Polygon _loadPolygon(const spk::JSON::Reader &reader, const std::string &shapeName);
		static void _orientPolygon(Polygon &polygon);
		void _appendPolygon(Polygon polygon);
		void _loadPolygons(const spk::JSON::Reader &reader);

	public:
		std::string name;
		std::vector<Polygon> polygons;
		std::set<std::string> materialSlots;
		std::array<bool, 6> coversSide{};

		explicit Shape(const spk::JSON::Reader &reader);
	};
}
