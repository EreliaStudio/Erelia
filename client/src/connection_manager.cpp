#include "erelia/client/connection_manager.hpp"

#include "erelia/client/service.hpp"
#include "erelia/core/service.hpp"

#include <core/context/update_context.hpp>
#include <diagnostics/logger.hpp>
#include <exception.hpp>
#include <network/client.hpp>
#include <system/translator.hpp>
#include <threading/worker_pool.hpp>

#include <chrono>
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
		throw spk::Exception(
			Service::translator()->translate(
				"client.connection.endpoint.address_empty"));
	}
	if (_endpoint.port == 0)
	{
		throw spk::Exception(
			Service::translator()->translate(
				"client.connection.endpoint.port_zero"));
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

	if (Service::client()->isConnected() == true)
	{
		Service::client()->disconnect();
	}
}

void ConnectionManager::_launchAttempt()
{
	if (
		Service::client()->isConnected() == true ||
		_connectionAttempt.has_value() == true)
	{
		return;
	}

	++_attemptCount;
	_retryTimer.reset();

	SPK_LOG(Info)
		<< Service::translator()->translate(
			"client.connection.attempt",
			_attemptCount,
			MaximumAttemptCount)
		<< std::endl;

	const Endpoint endpoint = _endpoint;
	_connectionAttempt.emplace(
		Service::workerPool()->submit([endpoint] {
			Service::client()->connect(endpoint.address, endpoint.port);
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

	_retryTimer.reset();
	_retryTimer.start();

	const auto retryDelay =
		std::chrono::duration_cast<std::chrono::milliseconds>(
			_retryTimer.duration());

	SPK_LOG(Warning)
		<< Service::translator()->translate(
			"client.connection.retry",
			retryDelay.count())
		<< std::endl;
}

void ConnectionManager::_stopCycle()
{
	_retryTimer.reset();

	SPK_LOG(Error)
		<< Service::translator()->translate(
			"client.connection.maximum_attempts_reached",
			MaximumAttemptCount)
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

	const bool completed =
		status == ConnectionTask::Status::Completed;
	const bool connected =
		completed == true &&
		Service::client()->isConnected() == true;

	_connectionAttempt.reset();

	if (connected == true)
	{
		_attemptCount = 0;
		_retryTimer.reset();
		SPK_LOG(Info)
			<< Service::translator()->translate(
				"client.connection.connected")
			<< std::endl;
		return;
	}

	if (completed == true)
	{
		_attemptCount = 0;
		_retryTimer.reset();
		SPK_LOG(Warning)
			<< Service::translator()->translate(
				"client.connection.lost")
			<< std::endl;
		connect();
		return;
	}

	_scheduleRetry();
}

void ConnectionManager::_updateState(spk::UpdateContext &)
{
	_processAttempt();

	if (
		Service::client()->isConnected() == false &&
		_connectionAttempt.has_value() == false &&
		_retryTimer.state() == spk::Timer::State::Off &&
		_attemptCount == 0)
	{
		SPK_LOG(Warning)
			<< Service::translator()->translate(
				"client.connection.lost")
			<< std::endl;
		connect();
		return;
	}

	if (_retryTimer.state() == spk::Timer::State::TimedOut)
	{
		_launchAttempt();
	}
}

void ConnectionManager::connect()
{
	if (Service::client()->isConnected() == true)
	{
		SPK_LOG(Info)
			<< Service::translator()->translate(
				"client.connection.already_connected")
			<< std::endl;
		return;
	}
	if (
		_connectionAttempt.has_value() == true &&
		_connectionAttempt->status() == ConnectionTask::Status::Pending)
	{
		SPK_LOG(Info)
			<< Service::translator()->translate(
				"client.connection.attempt_already_running")
			<< std::endl;
		return;
	}

	_attemptCount = 0;
	_retryTimer.reset();
	_connectionAttempt.reset();
	_launchAttempt();
}

bool ConnectionManager::isCycleStopped() const noexcept
{
	return
		_attemptCount >= MaximumAttemptCount &&
		_connectionAttempt.has_value() == false &&
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
		throw spk::Exception(
			Service::translator()->translate(
				"client.connection.endpoint.address_empty"));
	}
	if (endpoint.port == 0)
	{
		throw spk::Exception(
			Service::translator()->translate(
				"client.connection.endpoint.port_zero"));
	}

	if (
		Service::client()->isConnected() == true &&
		_endpoint.address == endpoint.address &&
		_endpoint.port == endpoint.port)
	{
		SPK_LOG(Info)
			<< Service::translator()->translate(
				"client.connection.already_connected")
			<< std::endl;
		return;
	}

	if (Service::client()->isConnected() == true)
	{
		Service::client()->disconnect();
	}

	_endpoint = std::move(endpoint);
	connect();
}

const ConnectionManager::Endpoint &ConnectionManager::endpoint() const noexcept
{
	return _endpoint;
}
