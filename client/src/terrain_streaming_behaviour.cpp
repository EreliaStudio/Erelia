#include "erelia/client/terrain_streaming_behaviour.hpp"
#include "erelia/client/service.hpp"
#include <algorithm>
#include <cmath>
#include <diagnostics/logger.hpp>
#include <limits>
namespace
{
	std::vector<Column::Coordinate> offsets(std::int32_t range)
	{
		std::vector<Column::Coordinate> result;
		for (std::int64_t x = -static_cast<std::int64_t>(range); x <= range; ++x)
		{
			for (std::int64_t z = -static_cast<std::int64_t>(range); z <= range; ++z)
			{
				result.push_back({static_cast<std::int32_t>(x), static_cast<std::int32_t>(z)});
			}
		}
		return result;
	}
	std::int32_t cell(float position)
	{
		const double value = std::floor(static_cast<double>(position));
		if (std::isfinite(value) == false || value < std::numeric_limits<std::int32_t>::min() || value > std::numeric_limits<std::int32_t>::max())
		{
			throw spk::Exception("Player position outside terrain coordinate domain");
		}
		return static_cast<std::int32_t>(value);
	}
}
void TerrainStreamingBehaviour::Ranges::validate() const
{
	if (viewRange <= 0 || unloadRange < viewRange)
	{
		throw spk::Exception("Terrain ranges require 0 < viewRange <= unloadRange");
	}
}
TerrainStreamingBehaviour::TerrainStreamingBehaviour(World::Columns &columns, World::Chunks &chunks, Ranges ranges) :
	spk::Behaviour3D("TerrainStreaming"),
	_columns(columns),
	_chunks(chunks),
	_ranges(ranges)
{
	_ranges.validate();
	_viewOffsets = offsets(ranges.viewRange);
	_viewRegion.insert(_viewOffsets.begin(), _viewOffsets.end());
	const auto unload = offsets(ranges.unloadRange);
	_unloadRegion.insert(unload.begin(), unload.end());
}
void TerrainStreamingBehaviour::attach(spk::Entity *entity)
{
	_transformContract.resign();
	spk::Behaviour3D::attach(entity);
	if (owner() == nullptr)
	{
		return;
	}
	_transformContract = owner()->transform().subscribeToEdition([this](const spk::Transform3D &transform) {
		_refresh(transform);
	});
	_refresh(owner()->transform());
}
bool TerrainStreamingBehaviour::_inside(Column::Coordinate coordinate, std::int32_t range) const
{
	if (_center.has_value() == false)
	{
		return false;
	}
	const auto x = static_cast<std::int64_t>(coordinate.x) - _center->x;
	const auto z = static_cast<std::int64_t>(coordinate.z) - _center->z;
	if (std::abs(x) > range || std::abs(z) > range)
	{
		return false;
	}
	const auto &region = range == _ranges.viewRange ? _viewRegion : _unloadRegion;
	return region.contains({static_cast<std::int32_t>(x), static_cast<std::int32_t>(z)});
}
void TerrainStreamingBehaviour::_refresh(const spk::Transform3D &transform)
{
	const auto position = transform.position(spk::ReferenceFrame::World);
	const auto center = Chunk::toCoordinate({cell(position.x), cell(position.y), cell(position.z)});
	if (_center.has_value() == true && *_center == center)
	{
		return;
	}
	_center = center;
	Service::clientEventCenter().playerChangedChunkEvent().trigger(center);
	for (const auto &key : _columns.keys())
	{
		if (_inside(key, _ranges.unloadRange) == false)
		{
			_columns.remove(key);
		}
	}
	for (const auto &key : _chunks.keys())
	{
		if (_inside({key.x, key.z}, _ranges.unloadRange) == false)
		{
			_chunks.remove(key);
		}
	}
	std::vector<Column::Coordinate> desired;
	for (const auto &offset : _viewOffsets)
	{
		desired.push_back({center.x + offset.x, center.z + offset.z});
	}
	_requestColumns(desired);
	dispatch();
}
void TerrainStreamingBehaviour::_requestColumns(const std::vector<Column::Coordinate> &keys)
{
	const auto group = _columns.request(keys);
	for (std::size_t index = 0; index < keys.size(); ++index)
	{
		const auto answer = group.at(index);
		auto producer = _columnCompletions.producer();
		auto published = std::make_shared<std::atomic_bool>(false);
		auto contract = answer.subscribeToCompletion([producer, key = keys[index], answer, published]() mutable {
			producer.publish({key, answer});
			published->store(true);
		});
		_columnSubscriptions.push_back({answer, std::move(contract), std::move(published)});
	}
}
void TerrainStreamingBehaviour::_requestChunks(const Column &column)
{
	std::vector<Chunk::Coordinate> desired;
	for (const auto &key : column.chunks)
	{
		if (_inside({key.x, key.z}, _ranges.viewRange) == true)
		{
			desired.push_back(key);
		}
	}
	const auto group = _chunks.request(desired);
	for (std::size_t index = 0; index < desired.size(); ++index)
	{
		const auto answer = group.at(index);
		auto producer = _chunkCompletions.producer();
		auto published = std::make_shared<std::atomic_bool>(false);
		auto contract = answer.subscribeToCompletion([producer, key = desired[index], answer, published]() mutable {
			producer.publish({key, answer});
			published->store(true);
		});
		_chunkSubscriptions.push_back({answer, std::move(contract), std::move(published)});
	}
}
void TerrainStreamingBehaviour::_updateState(spk::UpdateContext &)
{
	dispatch();
}
void TerrainStreamingBehaviour::dispatch()
{
	for (const auto &completion : _columnCompletions.drain(_drainedColumns))
	{
		if (completion.answer.status() == spk::Task<Column>::Status::Completed && _inside(completion.key, _ranges.viewRange) == true)
		{
			_requestChunks(completion.answer.result());
		}
	}
	_drainedColumns.clear();
	for (const auto &completion : _chunkCompletions.drain(_drainedChunks))
	{
		if (completion.answer.status() == spk::Task<Chunk>::Status::Completed && _inside({completion.key.x, completion.key.z}, _ranges.unloadRange) == true)
		{
			SPK_LOG(UserValueB) << "Chunk acquired: " << completion.key << std::endl;
		}
	}
	_drainedChunks.clear();
	std::erase_if(_columnSubscriptions, [](const auto &subscription) {
		return subscription.published->load() == true;
	});
	std::erase_if(_chunkSubscriptions, [](const auto &subscription) {
		return subscription.published->load() == true;
	});
}
