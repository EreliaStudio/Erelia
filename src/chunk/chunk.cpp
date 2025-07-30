#include "chunk.hpp"

BakableChunk::BakableChunk(const spk::Vector2Int &coordinates)
{
	setCoordinates(coordinates);
}

spk::ContractProvider::Contract BakableChunk::subscribeToEdition(const spk::ContractProvider::Job &p_job)
{
	return _onEditionContractProvider.subscribe(p_job);
}

void BakableChunk::setCoordinates(const spk::Vector2Int &coordinates)
{
	_coordinates = coordinates;
}

const spk::Vector2Int& BakableChunk::coordinates() const
{
	return _coordinates;
}
