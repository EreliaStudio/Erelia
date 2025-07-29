#pragma once

#include <sparkle.hpp>

#include "player.hpp"
#include "world.hpp"

struct Context
{
	spk::GameEngine engine;

	Player player;
	World world;

	Context();
};