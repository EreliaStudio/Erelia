#include "erelia/client/connection_manager.hpp"

#include <core/context/update_context.hpp>
#include <design_pattern/singleton.hpp>
#include <diagnostics/logger.hpp>
#include <exception.hpp>

#include <utility>

ConnectionManager::ConnectionManager(
	std::string name,
	Endpoint endpoint,
	spk::Widget *parent) :
	spk::Widget(std::move(name), parent),
	_endpoint(std::move(endpoint)),
	_client(spk::Singleton<spk::Client>::instance()),
	_workerPool(spk::Singleton<spk::WorkerPool>::instance()),
	_connectionContract(_client.subscribeToConnection([this] {
		SPK_LOG(Info) << "Connected to dedicated Server" << std::endl;
	})),
	_disconnectionContract(_client.subscribeToDisconnection([this] {
		_disconnected.store(true, std::memory_order_release);
	}))
{
	if (_endpoint.address.empty() == true)
	{
		throw spk::Exception("Client Server address cannot be empty");
	}
	if (_endpoint.port == 0)
	{
		throw spk::Exception("Client Server port cannot be zero");
	}

	activate();
	connect();
}

ConnectionManager::~ConnectionManager()
{
	if (
		_connectionAttempt.has_value() == true &&
		_connectionAttempt->status() == ConnectionTask::Status::Pending)
	{
		_connectionAttempt->wait();
	}

	if (_client.isConnected() == true)
	{
		_client.disconnect();
	}
}

void ConnectionManager::_launchAttempt()
{
	if (_client.isConnected() == true || _connectionAttempt.has_value() == true)
	{
		return;
	}

	++_attemptCount;
	_retryScheduled = false;
	_retryElapsed = {};
	SPK_LOG(Info)
		<< "Connecting to dedicated Server (attempt "
		<< _attemptCount << '/' << MaximumAttemptCount << ')'
		<< std::endl;

	_connectionAttempt.emplace(
		_workerPool.submit([this] {
			_client.connect(_endpoint.address, _endpoint.port);
			return true;
		}));
}

void ConnectionManager::_scheduleRetry()
{
	if (_attemptCount >= MaximumAttemptCount)
	{
		_stopCycle();
		return;
	}

	_retryElapsed = {};
	_retryScheduled = true;
	SPK_LOG(Warning)
		<< "Dedicated Server connection attempt failed; retrying in "
		<< RetryDelay.count() << " seconds"
		<< std::endl;
}

void ConnectionManager::_stopCycle()
{
	_retryScheduled = false;
	_cycleStopped = true;
	SPK_LOG(Error)
		<< "Unable to connect to dedicated Server after "
		<< MaximumAttemptCount
		<< " attempts; automatic connection attempts stopped. Use /connect to start a new connection cycle"
		<< std::endl;
}

void ConnectionManager::_processAttempt()
{
	if (_connectionAttempt.has_value() == false)
	{
		return;
	}

	const ConnectionTask::Status status = _connectionAttempt->status();
	if (status == ConnectionTask::Status::Pending)
	{
		return;
	}

	const bool connected =
		status == ConnectionTask::Status::Completed &&
		_client.isConnected() == true;
	_connectionAttempt.reset();

	if (connected == false)
	{
		_scheduleRetry();
	}
}

void ConnectionManager::_updateState(spk::UpdateContext &context)
{
	if (_disconnected.exchange(false, std::memory_order_acq_rel) == true)
	{
		SPK_LOG(Warning) << "Dedicated Server connection was lost" << std::endl;
		connect();
	}

	_processAttempt();

	if (_retryScheduled == true)
	{
		_retryElapsed += context.deltaTime;
		if (_retryElapsed >= RetryDelay)
		{
			_launchAttempt();
		}
	}
}

void ConnectionManager::connect()
{
	if (_client.isConnected() == true)
	{
		SPK_LOG(Info) << "Client is already connected to the dedicated Server" << std::endl;
		return;
	}
	if (
		_connectionAttempt.has_value() == true &&
		_connectionAttempt->status() == ConnectionTask::Status::Pending)
	{
		SPK_LOG(Info) << "A dedicated Server connection attempt is already running" << std::endl;
		return;
	}

	_attemptCount = 0;
	_retryElapsed = {};
	_retryScheduled = false;
	_cycleStopped = false;
	_connectionAttempt.reset();
	_launchAttempt();
}

bool ConnectionManager::isCycleStopped() const noexcept
{
	return _cycleStopped;
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

	if (
		_client.isConnected() == true &&
		_endpoint.address == endpoint.address &&
		_endpoint.port == endpoint.port)
	{
		SPK_LOG(Info) << "Client is already connected to the dedicated Server" << std::endl;
		return;
	}

	if (_client.isConnected() == true)
	{
		_client.disconnect();
		_disconnected.store(false, std::memory_order_release);
	}

	_endpoint = std::move(endpoint);
	connect();
}

const ConnectionManager::Endpoint &ConnectionManager::endpoint() const noexcept
{
	return _endpoint;
}
