#pragma once

#include "erelia/client/connection_manager.hpp"

#include <core/application.hpp>

#include <chrono>
#include <memory>
#include <string>

class MainApplicationWidget;

class EreliaClientApplication final : public spk::Application
{
private:
	std::unique_ptr<MainApplicationWidget> _mainWidget;

public:
	EreliaClientApplication(
		ConnectionManager::Endpoint endpoint,
		std::chrono::milliseconds retryDelay);
	~EreliaClientApplication();

	[[nodiscard]] MainApplicationWidget &mainWidget() noexcept;
	[[nodiscard]] const MainApplicationWidget &mainWidget() const noexcept;
};

struct ClientConfiguration
{
	ConnectionManager::Endpoint server;
	std::chrono::milliseconds retryDelay;

	[[nodiscard]] static ClientConfiguration load(const std::string &path);
};

[[nodiscard]] int runClient(int argc, char **argv);
