#pragma once

#include <cstdint>

#include "erelia/core/networking/collection_protocol_types.hpp"
#include "erelia/core/voxel/cell.hpp"
#include "erelia/core/voxel/volume.hpp"

struct Chunk : public Voxel::Volume
{
	using Coordinate = spk::Vector3Int;
	struct Protocol
	{
		using MessageTypes = Networking::CollectionProtocol::MessageTypes<
			Networking::MessageType::ChunkRequest,
			Networking::MessageType::ChunkResponse,
			Networking::MessageType::ChunkUpdate,
			Networking::MessageType::ChunkError>;
	};

	class Builder;

	inline static constexpr std::int32_t Extent = 16;

	Chunk();
	inline static constexpr std::size_t MaximumElementsPerRequest = 1024;
	inline static constexpr std::size_t ElementsPerResponseSection = 32;

	explicit Chunk(Voxel::Volume &&volume);

	[[nodiscard]] static Coordinate toCoordinate(const Voxel::Cell::Coordinate &globalCell) noexcept;
	[[nodiscard]] static Voxel::Volume::LocalCoordinate toLocalCoordinate(
		const Voxel::Cell::Coordinate &globalCell) noexcept;

private:
	explicit Chunk(Voxel::Volume::Buffer::Lease cells);

	friend class Builder;
};

spk::Message::Writer &operator<<(spk::Message::Writer &writer, const Chunk &chunk);
const spk::Message::Reader &operator>>(const spk::Message::Reader &reader, Chunk &chunk);
