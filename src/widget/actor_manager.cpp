#include "widget/actor_manager.hpp"

#include "structure/context.hpp"

void ActorManager::_onUpdateEvent(spk::UpdateEvent& p_event)
{
	if (p_event.deltaTime.milliseconds == 0)
	{
		return ;
	}

	for (auto& [id, actor] : Context::instance()->actorMap.actors())
	{
		actor->update();
	}
}

ActorManager::ActorManager(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent) :
	GraphicalWidget(p_name, p_parent)
{
	
}

void ActorManager::initialize()
{

}