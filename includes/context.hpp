#pragma once

#include <sparkle.hpp>

#include "player.hpp"
#include "node.hpp"
#include "world.hpp"

struct Context : public spk::Singleton<Context>
{
	friend class spk::Singleton<Context>;

private:
	Context(const Context &) = delete;
	Context &operator=(const Context &) = delete;

	Context();

	void populateNodeMap();

public:
	spk::GameEngine engine;

	NodeCollection nodes;

	Player player;
	World world;
};