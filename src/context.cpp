#include "context.hpp"

Context::Context() :
	player(L"Player", nullptr),
	world(L"World", nullptr)
{
	player.behavior().place({0, 0, 0});
	player.activate();

	world.activate();
	
	engine.addEntity(&player);
	engine.addEntity(&world);
}