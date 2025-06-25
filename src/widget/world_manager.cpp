#include "widget/world_manager.hpp"

void WorldManager::_onGeometryChange()
{
	spk::Vector2 topLeftCell = convertScreenToWorldPosition({0, 0});
	spk::Vector2 downRightCell = convertScreenToWorldPosition(geometry().size);
	spk::Vector2Int topLeftCorner = Tilemap::worldToChunk(spk::Vector3Int(topLeftCell, 0)) - 1;
	spk::Vector2Int downRightCorner = Tilemap::worldToChunk(spk::Vector3Int(downRightCell, 0)) + 1;
	
	_activeChunks.clear();

	for (int x = topLeftCorner.x; x <= downRightCorner.x; x++)
	{
		for (int y = topLeftCorner.y; y <= downRightCorner.y; y++)
		{
			Tilemap::ChunkCoordinate tmp = {x, y};

			if (Context::instance()->tilemap.contains(tmp) == false)
			{
				Context::instance()->tilemap.requestChunk(tmp);
			}

			spk::SafePointer<Chunk> tmpChunk = Context::instance()->tilemap.chunk(tmp);


			if (tmpChunk->isBaked() == false)
			{
				tmpChunk->bake();
			}

			spk::cout << "Add chunk : " << tmp << std::endl;
			_activeChunks.push_back(tmpChunk);
		}
	}
}

void WorldManager::_onPaintEvent(spk::PaintEvent& p_event)
{
	for (auto& chunk : _activeChunks)
	{
		spk::cout << "Rendering chunk : " << chunk->position() << std::endl;
		chunk->render();
	}
}

WorldManager::WorldManager(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent) :
	GraphicalWidget(p_name, p_parent)
{
	
}

void WorldManager::initialize()
{

}