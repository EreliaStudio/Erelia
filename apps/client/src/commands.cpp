#include "erelia/client/commands.hpp"
#include <diagnostics/logger.hpp>
#include <exception>
#include <string>

namespace erelia::client
{
    Commands::Commands(PingController &controller) : _controller(controller)
    {
        _parser.addCommand({
            .name = "ping",
            .description = "Ping a remote server",
            .parameters = {{.name = "server", .description = "A or B"}},
            .callback = [this](const spk::CommandParser::Invocation &invocation) {
                const auto &server = invocation.get("server").front();
                if (server != "A" && server != "B") throw std::invalid_argument("Expected server A or B");
                if (!_controller.ping(server.front())) throw std::logic_error("Ping already pending");
            }
        });
    }
    bool Commands::execute(const std::string &command, const std::string &source)
    {
        // Do not log arbitrary command text: future commands may accept credentials.
        const bool safe = command == "/ping A" || command == "/ping B";
        SPK_LOG(Info) << "COMMAND_ATTEMPT source=" << source << " text="
                      << (safe ? command : "<redacted>") << std::endl;
        try
        {
            const auto result = _parser.execute(command);
            const bool accepted = result.status == spk::CommandParser::Status::Accepted;
            SPK_LOG(Info) << "COMMAND_RESULT source=" << source << " status="
                          << (accepted ? "Accepted" : "Rejected") << std::endl;
            return accepted;
        }
        catch (const std::exception &e)
        {
            SPK_LOG(Warning) << "COMMAND_RESULT source=" << source
                             << " status=Failed reason=" << e.what() << std::endl;
            return false;
        }
    }
}
