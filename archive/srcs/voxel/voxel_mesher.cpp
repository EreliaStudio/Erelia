#include "voxel/voxel_mesher.hpp"

#include <cstdint>
#include <functional>
#include <span>
#include <unordered_map>
#include <utility>
#include <vector>

#include "voxel/voxel_mesher_occlusion.hpp"
#include "voxel/voxel_transform.hpp"

namespace voxel
{
	namespace
	{
		void combineHash(std::size_t &result, std::size_t value) noexcept
		{
			result ^= value + 0x9e3779b9 + (result << 6) + (result >> 2);
		}

		spk::Vector3 scalePosition(spk::Vector3 position, float scale) noexcept
		{
			return {position.x * scale, position.y * scale, position.z * scale};
		}

		template <typename TBuilder>
		void appendTriangles(TBuilder &builder, std::span<const std::uint32_t> indices, bool mirrored)
		{
			for (std::size_t index = 1; index + 1 < indices.size(); ++index)
				if (mirrored) builder.addTriangle(indices[0], indices[index + 1], indices[index]);
				else builder.addTriangle(indices[0], indices[index], indices[index + 1]);
		}
	}

	class VoxelMesher::PolygonBuilder
	{
	public:
		virtual ~PolygonBuilder() = default;
		virtual void append(const VoxelVolume &volume, const Voxel::Shape::Polygon &polygon,
			const Voxel::Definition &definition, spk::Vector3Int coordinate, Voxel::Cell cell) = 0;
	};

	class VoxelMesher::TextureIndexingBuilder final : public PolygonBuilder
	{
	private:
		struct VertexHash
		{
			[[nodiscard]] std::size_t operator()(const spk::Texture3DVertex &vertex) const noexcept;
		};
		struct VertexEqual
		{
			[[nodiscard]] bool operator()(const spk::Texture3DVertex &left,
				const spk::Texture3DVertex &right) const noexcept;
		};

		spk::TextureMesh3D::Builder _builder;
		const Voxel::Catalog<Voxel::Definition> &_catalog;
		std::unordered_map<spk::Texture3DVertex, spk::TextureMesh3D::Index,
			VertexHash, VertexEqual> _indices;

		[[nodiscard]] spk::TextureMesh3D::Index _addVertex(const spk::Texture3DVertex &vertex);

	public:
		explicit TextureIndexingBuilder(const Voxel::Catalog<Voxel::Definition> &catalog);
		void append(const VoxelVolume &volume, const Voxel::Shape::Polygon &polygon,
			const Voxel::Definition &definition, spk::Vector3Int coordinate, Voxel::Cell cell) override;
		void addTriangle(std::uint32_t a, std::uint32_t b, std::uint32_t c);
		[[nodiscard]] spk::TextureMesh3D build() &&;
	};

	class VoxelMesher::PaletteIndexingBuilder final : public PolygonBuilder
	{
	private:
		struct VertexHash
		{
			[[nodiscard]] std::size_t operator()(const VoxelVertex &vertex) const noexcept;
		};

		VoxelMesh::Builder _builder;
		const MaterialResolver &_materialResolver;
		std::unordered_map<VoxelVertex, VoxelMesh::Index, VertexHash> _indices;

		[[nodiscard]] VoxelMesh::Index _addVertex(const VoxelVertex &vertex);

	public:
		explicit PaletteIndexingBuilder(const MaterialResolver &materialResolver);
		void append(const VoxelVolume &volume, const Voxel::Shape::Polygon &polygon,
			const Voxel::Definition &definition, spk::Vector3Int coordinate, Voxel::Cell cell) override;
		void addTriangle(std::uint32_t a, std::uint32_t b, std::uint32_t c);
		[[nodiscard]] VoxelMesh build() &&;
	};

	std::size_t VoxelMesher::TextureIndexingBuilder::VertexHash::operator()(
		const spk::Texture3DVertex &vertex) const noexcept
	{
		std::size_t result = std::hash<spk::Vector3>{}(vertex.position);
		combineHash(result, std::hash<spk::Vector3>{}(vertex.normal));
		combineHash(result, std::hash<spk::Vector2>{}(vertex.uv));
		return result;
	}

	bool VoxelMesher::TextureIndexingBuilder::VertexEqual::operator()(
		const spk::Texture3DVertex &left, const spk::Texture3DVertex &right) const noexcept
	{
		return left.position == right.position && left.normal == right.normal && left.uv == right.uv;
	}

	VoxelMesher::TextureIndexingBuilder::TextureIndexingBuilder(
		const Voxel::Catalog<Voxel::Definition> &catalog) :
		_catalog(catalog)
	{
	}

	spk::TextureMesh3D::Index VoxelMesher::TextureIndexingBuilder::_addVertex(
		const spk::Texture3DVertex &vertex)
	{
		if (const auto iterator = _indices.find(vertex); iterator != _indices.end())
			return iterator->second;
		const auto index = _builder.addVertex(vertex);
		_indices.emplace(vertex, index);
		return index;
	}

	void VoxelMesher::TextureIndexingBuilder::append(const VoxelVolume &volume,
		const Voxel::Shape::Polygon &polygon, const Voxel::Definition &definition,
		spk::Vector3Int coordinate, Voxel::Cell cell)
	{
		const auto atlas = definition.textures.at(polygon.materialSlot);
		const auto sprites = _catalog.atlas().nbSprite();
		const spk::Vector2 unit{1.0f / static_cast<float>(sprites.x), 1.0f / static_cast<float>(sprites.y)};
		std::vector<std::uint32_t> indices;
		indices.reserve(polygon.vertices.size());
		for (const auto &vertex : polygon.vertices)
		{
			const spk::Vector3 local = vertex.position + spk::Vector3(coordinate);
			const auto position = scalePosition(local, volume.voxelSize());
			const spk::Vector2 uv{(atlas.x + vertex.uv.x) * unit.x, (atlas.y + vertex.uv.y) * unit.y};
			indices.push_back(_addVertex({position, polygon.normal, uv}));
		}
		appendTriangles(*this, indices, cell.flip() == Voxel::Flip::NegativeY);
	}

	void VoxelMesher::TextureIndexingBuilder::addTriangle(
		std::uint32_t a, std::uint32_t b, std::uint32_t c)
	{
		_builder.addTriangle(a, b, c);
	}

	spk::TextureMesh3D VoxelMesher::TextureIndexingBuilder::build() &&
	{
		return std::move(_builder).build();
	}

	std::size_t VoxelMesher::PaletteIndexingBuilder::VertexHash::operator()(
		const VoxelVertex &vertex) const noexcept
	{
		std::size_t result = std::hash<spk::Vector3>{}(vertex.position);
		combineHash(result, std::hash<spk::Vector3>{}(vertex.normal));
		combineHash(result, std::hash<std::uint32_t>{}(vertex.paletteElementIndex));
		return result;
	}

	VoxelMesher::PaletteIndexingBuilder::PaletteIndexingBuilder(
		const MaterialResolver &materialResolver) :
		_materialResolver(materialResolver)
	{
	}

	VoxelMesh::Index VoxelMesher::PaletteIndexingBuilder::_addVertex(const VoxelVertex &vertex)
	{
		if (const auto iterator = _indices.find(vertex); iterator != _indices.end())
			return iterator->second;
		const auto index = _builder.addVertex(vertex);
		_indices.emplace(vertex, index);
		return index;
	}

	void VoxelMesher::PaletteIndexingBuilder::append(const VoxelVolume &volume,
		const Voxel::Shape::Polygon &polygon, const Voxel::Definition &definition,
		spk::Vector3Int coordinate, Voxel::Cell cell)
	{
		const auto elementIndex = _materialResolver.resolve({volume, definition, polygon, coordinate, cell});
		std::vector<std::uint32_t> indices;
		indices.reserve(polygon.vertices.size());
		for (const auto &vertex : polygon.vertices)
		{
			const spk::Vector3 local = vertex.position + spk::Vector3(coordinate);
			const auto position = scalePosition(local, volume.voxelSize());
			indices.push_back(_addVertex({position, polygon.normal, elementIndex}));
		}
		appendTriangles(*this, indices, cell.flip() == Voxel::Flip::NegativeY);
	}

	void VoxelMesher::PaletteIndexingBuilder::addTriangle(
		std::uint32_t a, std::uint32_t b, std::uint32_t c)
	{
		_builder.addTriangle(a, b, c);
	}

	VoxelMesh VoxelMesher::PaletteIndexingBuilder::build() &&
	{
		return std::move(_builder).build();
	}

	VoxelMesher::VoxelMesher(const Voxel::Catalog<Voxel::Definition> &catalog) :
		_catalog(catalog),
		_occlusionCache(std::make_unique<OcclusionCache>())
	{
	}

	VoxelMesher::~VoxelMesher() = default;

	spk::TextureMesh3D VoxelMesher::bake(const VoxelVolume &volume) const
	{
		_validateDefinitions(volume);
		TextureIndexingBuilder builder(_catalog);
		_appendCells(builder, volume);
		return std::move(builder).build();
	}

	VoxelMesh VoxelMesher::bake(
		const VoxelVolume &volume, const MaterialResolver &materialResolver) const
	{
		_validateDefinitions(volume);
		PaletteIndexingBuilder builder(materialResolver);
		_appendCells(builder, volume);
		return std::move(builder).build();
	}

	void VoxelMesher::_validateDefinitions(const VoxelVolume &volume) const
	{
		for (const Voxel::Cell cell : volume.cells())
		{
			if (!cell.empty()) static_cast<void>(_catalog.definition(cell.id()));
		}
	}

	void VoxelMesher::_appendCells(PolygonBuilder &builder, const VoxelVolume &volume) const
	{
		const auto dimensions = volume.dimensions();
		for (std::uint32_t y = 0; y < dimensions.y; ++y)
			for (std::uint32_t z = 0; z < dimensions.z; ++z)
				for (std::uint32_t x = 0; x < dimensions.x; ++x)
				{
					const spk::Vector3Int coordinate{
						static_cast<int>(x), static_cast<int>(y), static_cast<int>(z)};
					const Voxel::Cell cell = volume.at(coordinate);
					if (!cell.empty()) _appendCell(builder, volume, coordinate, cell);
				}
	}

	void VoxelMesher::_appendCell(PolygonBuilder &builder, const VoxelVolume &volume,
		spk::Vector3Int coordinate, Voxel::Cell cell) const
	{
		const auto &definition = _catalog.definition(cell.id());
		for (const auto &polygon : definition.shape->polygons)
		{
			_appendVisiblePolygon(builder, volume, polygon, definition, coordinate, cell);
		}
	}

	Voxel::Cell VoxelMesher::_neighbor(const VoxelVolume &volume, spk::Vector3Int coordinate) const
	{
		return volume.contains(coordinate) ? volume.at(coordinate) : _outsideCell(volume, coordinate);
	}

	Voxel::Cell VoxelMesher::_outsideCell(const VoxelVolume &, spk::Vector3Int) const
	{
		return {};
	}

	void VoxelMesher::_appendVisiblePolygon(PolygonBuilder &builder, const VoxelVolume &volume,
		const Voxel::Shape::Polygon &polygon, const Voxel::Definition &definition,
		spk::Vector3Int coordinate, Voxel::Cell cell) const
	{
		const auto transformed = Voxel::transformPolygon(polygon, cell);
		if (transformed.outerSide == Voxel::Side::None)
		{
			_appendPolygon(builder, volume, transformed, definition, coordinate, cell);
			return;
		}
		const Voxel::Cell neighbor = _neighbor(volume, coordinate + Voxel::sideOffset(transformed.outerSide));
		if (neighbor.empty())
		{
			_appendPolygon(builder, volume, transformed, definition, coordinate, cell);
			return;
		}
		const auto &occludingShape = *_catalog.definition(neighbor.id()).shape;
		for (const auto &visible : _occlusionCache->visibleRemnants(polygon, cell, occludingShape, neighbor))
			_appendPolygon(builder, volume, visible, definition, coordinate, cell);
	}

	void VoxelMesher::_appendPolygon(PolygonBuilder &builder, const VoxelVolume &volume,
		const Voxel::Shape::Polygon &polygon, const Voxel::Definition &definition,
		spk::Vector3Int coordinate, Voxel::Cell cell) const
	{
		builder.append(volume, polygon, definition, coordinate, cell);
	}

	VoxelMesher::CacheStatistics VoxelMesher::_cacheStatistics() const noexcept
	{
		return _occlusionCache->statistics();
	}
}
