#include "widget/world_manager.hpp"

void WorldManager::_onGeometryChange()
{
	spk::Vector2 downLeftCell = convertScreenToWorldPosition({0, geometry().size.y});
	spk::Vector2 topRightCell = convertScreenToWorldPosition({geometry().size.x, 0});
	spk::Vector2Int downLeftCorner = Tilemap::worldToChunk(spk::Vector3Int(downLeftCell, 0)) - 1;
	spk::Vector2Int topRightCorner = Tilemap::worldToChunk(spk::Vector3Int(topRightCell, 0)) + 1;
	
	_activeChunks.clear();

	spk::cout << "Down Left : " << downLeftCell << std::endl;
	spk::cout << "Top right : " << topRightCell << std::endl;

	for (int x = downLeftCorner.x; x <= topRightCorner.x; x++)
	{
		for (int y = downLeftCorner.y; y <= topRightCorner.y; y++)
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

			_activeChunks.push_back(tmpChunk);
		}
	}
}

void WorldManager::_onPaintEvent(spk::PaintEvent& p_event)
{
	for (auto& chunk : _activeChunks)
	{
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