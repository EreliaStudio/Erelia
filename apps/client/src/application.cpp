#include "erelia/client/application.hpp"
#include "erelia/client/commands.hpp"
#include "erelia/client/command_socket.hpp"
#include "erelia/client/ping_controller.hpp"
#include "erelia/client/translator_service.hpp"
#include <core/application.hpp>
#include <network/client.hpp>
#include <ui/layout/grid_layout.hpp>
#include <ui/widget/push_button.hpp>
#include <ui/widget/text_label.hpp>
#include <diagnostics/logger.hpp>
#include <chrono>
#include <string>

namespace erelia::client
{
    namespace
    {
        const char *statusKey(PingStatus status)
        {
            switch (status)
            {
            case PingStatus::Idle: return "client.idle";
            case PingStatus::Waiting: return "client.waiting";
            case PingStatus::Received: return "client.received";
            case PingStatus::Failed: return "client.failed";
            }
            return "client.failed";
        }

        class PingView final : public spk::Widget
        {
            spk::Client &_network;
            PingController &_pings;
            Commands &_commands;
            CommandSocket &_socket;
            spk::TextLabel _aTitle{"A.title", this};
            spk::TextLabel _bTitle{"B.title", this};
            spk::TextLabel _aStatus{"A.status", this};
            spk::TextLabel _bStatus{"B.status", this};
            spk::PushButton _aButton{"A.button", this};
            spk::PushButton _bButton{"B.button", this};
            spk::GridLayoutFixedGrid<2, 2> _layout;
            spk::PushButton::ClickContract _aClick;
            spk::PushButton::ClickContract _bClick;
            PingStatus _lastA = PingStatus::Idle;
            PingStatus _lastB = PingStatus::Idle;
            bool _connected = false;
            void _onGeometryChange() override
            {
                _layout.setGeometry({.anchor = {20, 20}, .size = {600, 140}});
                _aButton.setGeometry({.anchor = {70, 195}, .size = {200, 60}});
                _bButton.setGeometry({.anchor = {370, 195}, .size = {200, 60}});
            }
            void _updateState(spk::UpdateContext &) override
            {
                if (!_connected && _network.isConnected())
                {
                    _connected = true;
                    SPK_LOG(Info) << "CLIENT_READY" << std::endl;
                }
                _socket.dispatch([this](const std::string &command) { return _commands.execute(command, "socket"); });
                _network.treatMessages();
                _pings.poll();
                const auto a = _pings.state('A').status, b = _pings.state('B').status;
                if (a != _lastA)
                {
                    _lastA = a;
                    _aStatus.setText(translatorService().translate(statusKey(a)));
                    SPK_LOG(Info) << "CLIENT_STATUS target=A status=" << statusKey(a) << std::endl;
                }
                if (b != _lastB)
                {
                    _lastB = b;
                    _bStatus.setText(translatorService().translate(statusKey(b)));
                    SPK_LOG(Info) << "CLIENT_STATUS target=B status=" << statusKey(b) << std::endl;
                }
                if (a == PingStatus::Waiting) _aButton.deactivate();
                else _aButton.activate();
                if (b == PingStatus::Waiting) _bButton.deactivate();
                else _bButton.activate();
            }
        public:
            PingView(spk::Widget *parent, spk::Client &network, PingController &pings, Commands &commands, CommandSocket &socket) :
                Widget("PingView", parent), _network(network), _pings(pings), _commands(commands), _socket(socket)
            {
                auto &translator = translatorService();
                _aTitle.setText(translator.translate("client.server_a"));
                _bTitle.setText(translator.translate("client.server_b"));
                _aStatus.setText(translator.translate("client.idle"));
                _bStatus.setText(translator.translate("client.idle"));
                _aButton.setText(translator.translate("client.ping_a"));
                _bButton.setText(translator.translate("client.ping_b"));
                _layout.setWidget(0, 0, &_aTitle);
                _layout.setWidget(1, 0, &_bTitle);
                _layout.setWidget(0, 1, &_aStatus);
                _layout.setWidget(1, 1, &_bStatus);
                _aClick = _aButton.subscribeToClick([this] { (void)_commands.execute("/ping A", "button"); });
                _bClick = _bButton.subscribeToClick([this] { (void)_commands.execute("/ping B", "button"); });
                setGeometry({.anchor = {0, 0}, .size = {640, 300}});
                activate();
            }
        };
    }

    int runApplication(const config::Client &configuration)
    {
        spk::Client network;
        PingController pings(std::chrono::milliseconds(configuration.timeoutMs), [&network](const auto &message) {
            network.send(message);
        });
        Commands commands(pings);
        auto aContract = network.messageDispatcher().subscribeTo(ping::ResponseA, [&pings](const auto &message) {
            (void)pings.receive(message);
        });
        auto bContract = network.messageDispatcher().subscribeTo(ping::ResponseB, [&pings](const auto &message) {
            (void)pings.receive(message);
        });
        CommandSocket socket(configuration.commandPort);
        spk::Application app;
        auto &window = app.createWindow("main", {
            .title = "Erelia M1",
            .area = {.anchor = {100, 100}, .size = {640, 300}}
        });
        PingView view(&window.root(), network, pings, commands, socket);
        network.connect(configuration.router.host, configuration.router.port);
        return app.run();
    }
}
