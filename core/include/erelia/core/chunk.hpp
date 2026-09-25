#pragma once

#include <cstdint>

#include "erelia/core/voxel/cell.hpp"
#include "erelia/core/voxel/volume.hpp"

struct Chunk : public Voxel::Volume
{
	using Coordinate = spk::Vector3Int;

	class Builder;
	class Collection;

	class Protocol final
	{
	public:
		class Request;
		class Error;
		class Response;
	};

	inline static constexpr std::int32_t Extent = 16;

	explicit Chunk(Voxel::Volume &&volume);

	[[nodiscard]] static Coordinate toCoordinate(const Voxel::Cell::Coordinate &globalCell) noexcept;
	[[nodiscard]] static Voxel::Volume::LocalCoordinate toLocalCoordinate(
		const Voxel::Cell::Coordinate &globalCell) noexcept;

private:
	explicit Chunk(Voxel::Volume::Buffer::Lease cells);

	friend class Builder;
};
