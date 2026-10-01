#include "erelia/client/client_network_manager.hpp"
#include "erelia/client/service.hpp"
#include <diagnostics/logger.hpp>
#include <exception.hpp>
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
	try
	{
		_disconnectionContract = Service::client().subscribeToDisconnection([flag = _disconnected] {
			flag->store(true);
		});
	} catch (...)
	{
		drainerOwned.store(false);
		throw;
	}
	activate();
}
ClientNetworkManager::~ClientNetworkManager()
{
	_disconnectionContract.resign();
	drainerOwned.store(false);
}
void ClientNetworkManager::_updateState(spk::UpdateContext &)
{
	dispatch();
}
void ClientNetworkManager::dispatch()
{
	if (_disconnected->exchange(false) == true)
	{
		_disconnect.trigger();
	}
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
