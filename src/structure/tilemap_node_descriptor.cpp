#include "structure/tilemap.hpp"

#include "atlas/asset_atlas.hpp"

static std::wstring nodeDescriptors = LR"({
	"Nodes": [
		{
			"ID":0,
			"Flag": ["Obstacle"],
			"Type": "Autotiled",
			"Sprite": {
				"X":0,
				"Y":0
			}
		},
		{
			"ID":1,
			"Flag": ["Obstacle"],
			"Type": "Autotiled",
			"Sprite": {
				"X":4,
				"Y":0
			}
		}
	]
})";

void Tilemap::_loadNodeMap()
{
	ChunkRenderer::setSpriteSheet(AssetAtlas::instance()->spriteSheet(L"ChunkSpriteSheet"));

    spk::JSON::File cfg = spk::JSON::File::loadFromString(nodeDescriptors);
    const auto &nodes = cfg[L"Nodes"].asArray();

    for (const spk::JSON::Object *nodePtr : nodes)
    {
        const auto &j = *nodePtr;

        const size_t id = j[L"ID"].as<long>();
        const Node::Type type = toNodeType(j[L"Type"].as<std::wstring>());

        spk::Vector2Int spritePos = j.contains(L"Sprite") ? j[L"Sprite"] : spk::Vector2Int{0,0};

        spk::Vector2Int animOffset = j.contains(L"AnimationOffset") ? j[L"AnimationOffset"] : spk::Vector2Int{0,0};
			
        Node::Flag flags = Node::Flag::None;
        if (j.contains(L"Flag"))
        {
            const auto &flagField = j[L"Flag"];
            if (flagField.isArray())
			{
                for (auto *f : flagField.asArray())
				{
					flags |= toNodeFlag(f->as<std::wstring>());
				}
			}
            else
			{
                flags = toNodeFlag(flagField.as<std::wstring>());
			}
        }

        const long frameDuration = j.contains(L"FrameDuration") ? j[L"FrameDuration"].as<long>() : 0;
        const size_t nbFrames = j.contains(L"NbFrame") ? j[L"NbFrame"].as<long>() : 1;

        _nodeMap.addNode(id, Node(flags, type, spritePos, animOffset, frameDuration, nbFrames));
    }
}