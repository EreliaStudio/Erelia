#include "structure/actor_map.hpp"

void ActorMap::clear()
{
	_actors.clear();
}

spk::SafePointer<Actor> ActorMap::addActor(ActorID p_id, std::unique_ptr<Actor> p_actor)
{
	_actors[p_id] = std::move(p_actor);
	return (actor(p_id));
}

bool ActorMap::contains(ActorID p_id) const
{
	return _actors.find(p_id) != _actors.end();
}

spk::SafePointer<Actor> ActorMap::actor(ActorID p_id) const
{
	return _actors.at(p_id).get();
}

spk::SafePointer<Actor> ActorMap::requestActor(ActorID p_id)
{
	if (contains(p_id) == false)
	{
		addActor(p_id, std::make_unique<Actor>());
	}
	return (actor(p_id));
}

void ActorMap::removeActor(ActorID p_id)
{
	_actors.erase(p_id);
}

const std::unordered_map<ActorMap::ActorID, std::unique_ptr<Actor>>& ActorMap::actors() const
{
	return (_actors);
}

std::unordered_map<ActorMap::ActorID, std::unique_ptr<Actor>>& ActorMap::actors()
{
	return (_actors);
}