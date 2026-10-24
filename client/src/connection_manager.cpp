#include "erelia/client/connection_manager.hpp"

#include "erelia/client/service.hpp"

#include <core/context/update_context.hpp>
#include <diagnostics/logger.hpp>
#include <exception.hpp>
#include <network/client.hpp>

#include <chrono>
#include <exception>
#include <utility>

ConnectionManager::ConnectionManager(
	std::string name,
	Endpoint endpoint,
	spk::Timer::Duration retryDelay,
	spk::Widget *parent) :
	spk::Widget(std::move(name), parent),
	_endpoint(std::move(endpoint)),
	_retryTimer(retryDelay)
{
	if (_endpoint.address.empty() == true)
	{
		throw spk::Exception("Client Server address cannot be empty");
	}
	if (_endpoint.port == 0)
	{
		throw spk::Exception("Client Server port cannot be zero");
	}

	_connectionRequestContract = Service::clientEventCenter().connectionRequested().subscribe(
		[this](const Client::ConnectionRequest &request) {
			_processConnectionRequest(request);
		});
	activate();
	connect();
}

ConnectionManager::~ConnectionManager()
{
	_connectionRequestContract.resign();
	Service::client().disconnect();
	(void)_processDisconnection();
}

bool ConnectionManager::_processDisconnection()
{
	if (_connected == false ||
		(_connection.has_value() == true && _connection->status() == spk::Connection::Status::Connected))
	{
		return false;
	}

	_connected = false;
	_connection.reset();
	Service::clientEventCenter().clientDisconnected().trigger();
	return true;
}

void ConnectionManager::_processConnectionRequest(const Client::ConnectionRequest &request)
{
	Endpoint endpoint = _endpoint;
	if (request.address.has_value() == true)
	{
		endpoint.address = *request.address;
	}
	if (request.port.has_value() == true)
	{
		endpoint.port = *request.port;
	}

	if (_endpoint.address == endpoint.address &&
		_endpoint.port == endpoint.port)
	{
		connect();
		return;
	}

	connect(std::move(endpoint));
}

void ConnectionManager::_launchAttempt()
{
	if (_connection.has_value() == true ||
		Service::client().isConnected() == true)
	{
		return;
	}

	++_attemptCount;
	_retryTimer.reset();

	SPK_LOG(Info)
		<< "Connecting to dedicated Server (attempt "
		<< _attemptCount << '/' << MaximumAttemptCount << ')'
		<< std::endl;

	try
	{
		_connection.emplace(
			Service::client().connect(_endpoint.address, _endpoint.port));
	} catch (const std::exception &exception)
	{
		SPK_LOG(Warning)
			<< "Dedicated Server connection attempt failed: "
			<< exception.what() << std::endl;
		_scheduleRetry();
	}
}

void ConnectionManager::_scheduleRetry()
{
	if (_attemptCount >= MaximumAttemptCount)
	{
		_stopCycle();
		return;
	}

	_retryTimer.reset();
	_retryTimer.start();

	const auto retryDelay =
		std::chrono::duration_cast<std::chrono::milliseconds>(
			_retryTimer.duration());

	SPK_LOG(Warning)
		<< "Dedicated Server connection attempt failed; retrying in "
		<< retryDelay.count() << " ms"
		<< std::endl;
}

void ConnectionManager::_stopCycle()
{
	_retryTimer.reset();

	SPK_LOG(Error)
		<< "Unable to connect to dedicated Server after "
		<< MaximumAttemptCount
		<< " attempts; automatic connection attempts stopped. Use /connect to start a new connection cycle"
		<< std::endl;
}

void ConnectionManager::_processConnection()
{
	if (_connection.has_value() == false || _connected == true)
	{
		return;
	}

	switch (_connection->status())
	{
	case spk::Connection::Status::Pending:
		return;
	case spk::Connection::Status::Connected:
		_connected = true;
		_attemptCount = 0;
		_retryTimer.reset();
		SPK_LOG(Info) << "Connected to dedicated Server" << std::endl;
		Service::clientEventCenter().clientConnected().trigger();
		return;
	case spk::Connection::Status::Disconnected:
		_connection.reset();
		_scheduleRetry();
		return;
	}
}

void ConnectionManager::_updateState(spk::UpdateContext &)
{
	if (_processDisconnection() == true)
	{
		_attemptCount = 0;
		_retryTimer.reset();
		_launchAttempt();
		return;
	}

	_processConnection();

	if (_retryTimer.state() == spk::Timer::State::TimedOut)
	{
		_launchAttempt();
	}
}

void ConnectionManager::connect()
{
	if (Service::client().isConnected() == true)
	{
		SPK_LOG(Info) << "Client is already connected to the dedicated Server" << std::endl;
		return;
	}
	if (_connection.has_value() == true &&
		_connection->status() == spk::Connection::Status::Pending)
	{
		SPK_LOG(Info) << "A dedicated Server connection attempt is already running" << std::endl;
		return;
	}

	(void)_processDisconnection();
	_connection.reset();
	_attemptCount = 0;
	_retryTimer.reset();
	_launchAttempt();
}

bool ConnectionManager::isCycleStopped() const noexcept
{
	return _attemptCount >= MaximumAttemptCount &&
		   _connection.has_value() == false &&
		   _retryTimer.state() == spk::Timer::State::Off;
}

std::size_t ConnectionManager::attemptCount() const noexcept
{
	return _attemptCount;
}

void ConnectionManager::connect(Endpoint endpoint)
{
	if (endpoint.address.empty() == true)
	{
		throw spk::Exception("Client Server address cannot be empty");
	}
	if (endpoint.port == 0)
	{
		throw spk::Exception("Client Server port cannot be zero");
	}

	if (_endpoint.address == endpoint.address &&
		_endpoint.port == endpoint.port)
	{
		connect();
		return;
	}

	Service::client().disconnect();
	(void)_processDisconnection();
	_connection.reset();
	_endpoint = std::move(endpoint);
	_attemptCount = 0;
	_retryTimer.reset();
	_launchAttempt();
}

const ConnectionManager::Endpoint &ConnectionManager::endpoint() const noexcept
{
	return _endpoint;
}
