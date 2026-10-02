#include "erelia/client/client_network_manager.hpp"
#include "erelia/client/service.hpp"
#include <atomic>
#include <diagnostics/logger.hpp>
#include <exception.hpp>
#include <network/client.hpp>
namespace
{
	std::atomic_bool drainerOwned = false;
}
ClientNetworkManager::ClientNetworkManager(spk::Widget *parent) :
	spk::Widget("ClientNetworkManager", parent)
{
	if (drainerOwned.exchange(true) == true)
	{
		throw spk::Exception("Client receive queue already has a network manager");
	}
	activate();
}
ClientNetworkManager::~ClientNetworkManager()
{
	drainerOwned.store(false);
}
void ClientNetworkManager::_updateState(spk::UpdateContext &)
{
	dispatch();
}
void ClientNetworkManager::dispatch()
{
	for (const auto &message : Service::client().messages().drain(_messages))
	{
		try
		{
			_dispatcher.dispatch(message.type(), message);
		} catch (const std::exception &error)
		{
			SPK_LOG(Warning) << "Malformed Collection Message: " << error.what() << std::endl;
		}
	}
	_messages.clear();
}
