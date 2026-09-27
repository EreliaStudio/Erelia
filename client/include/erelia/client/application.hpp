#pragma once

#include "erelia/client/connection_manager.hpp"

#include <core/application.hpp>

#include <memory>
#include <string>

class MainApplicationWidget;

class EreliaClientApplication final : public spk::Application
{
private:
	std::unique_ptr<MainApplicationWidget> _mainWidget;

public:
	explicit EreliaClientApplication(ConnectionManager::Endpoint endpoint);
	~EreliaClientApplication();
};

struct ClientConfiguration
{
	ConnectionManager::Endpoint server;

	[[nodiscard]] static ClientConfiguration load(const std::string &path);
};

[[nodiscard]] int runClient(int argc, char **argv);
