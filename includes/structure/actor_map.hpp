#pragma once

#include <unordered_map>
#include <memory>

#include <sparkle.hpp>

#include "structure/actor.hpp"

class ActorMap
{
public:
	using ActorID = long;

private:
	std::unordered_map<ActorID, std::unique_ptr<Actor>> _actors;

public:
	void clear();

	spk::SafePointer<Actor> addActor(ActorID p_id, std::unique_ptr<Actor> p_actor);

	bool contains(ActorID p_id) const;

	spk::SafePointer<Actor> actor(ActorID p_id) const;
	spk::SafePointer<Actor> requestActor(ActorID p_id);

	void removeActor(ActorID p_id);

	const std::unordered_map<ActorID, std::unique_ptr<Actor>>& actors() const;
	std::unordered_map<ActorID, std::unique_ptr<Actor>>& actors();
};