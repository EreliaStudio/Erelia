#pragma once

#include <sparkle.hpp>

struct Node
{
	enum class Type
	{
		Autotiled,
		Monotiled
	};

    enum class Flag : std::uint16_t
    {
        None         = 0,
        EastBlocked  = 1 << 0,
        WestBlocked  = 1 << 1,
        NorthBlocked = 1 << 2,
        SouthBlocked = 1 << 3,
        Obstacle     = EastBlocked | WestBlocked | NorthBlocked | SouthBlocked
    };

	using Flags = spk::Flags<Flag>;

    Flags flags;
    Type isAutotiled;
    spk::Vector2Int sprite;
	spk::Vector2Int animationOffset;
	float frameDuration;
	float nbFrame;

	Node(Flags p_flags = Flag::None, Type p_isAutotiled = Type::Monotiled, spk::Vector2Int p_sprite = {0, 0}, spk::Vector2Int p_animationOffset = {0, 0}, float p_frameDuration = 0, float p_nbFrame = 1) :
		flags(p_flags),
		isAutotiled(p_isAutotiled),
		sprite(p_sprite),
		animationOffset(p_animationOffset),
		frameDuration(p_frameDuration),
		nbFrame(p_nbFrame)
	{

	}
};

Node::Type toNodeType(const std::wstring& p_string);
Node::Flag toNodeFlag(const std::wstring& p_string);