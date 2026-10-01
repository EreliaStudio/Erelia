#pragma once
#include "erelia/core/collection.hpp"
#include "erelia/core/column.hpp"
#include "erelia/core/service.hpp"
class PrototypeChunkProvider final : public Collection<Chunk::Coordinate, Chunk>::GeneratingProvider
{
protected:
	[[nodiscard]] std::function<Chunk()> _operation(const Chunk::Coordinate &coordinate) const override;

public:
	PrototypeChunkProvider() :
		GeneratingProvider(Service::workerPool())
	{
	}
};
class PrototypeColumnProvider final : public Collection<Column::Coordinate, Column>::GeneratingProvider
{
protected:
	[[nodiscard]] std::function<Column()> _operation(const Column::Coordinate &coordinate) const override;

public:
	PrototypeColumnProvider() :
		GeneratingProvider(Service::workerPool())
	{
	}
};
