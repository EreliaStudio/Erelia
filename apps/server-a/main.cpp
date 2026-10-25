#include "erelia/server_a/server.hpp"
#include <erelia/shared/runtime.hpp>
#include <chrono>
#include <iostream>
#include <thread>

int main(int argc, char **argv)
{
    try
    {
        auto args = erelia::config::arguments(argc, argv, "server-a.json");
        auto cfg = erelia::config::server(args.file);
        if (args.validateOnly) return 0;
        auto log = erelia::runtime::logTo(cfg.logFile);
        erelia::runtime::signals();
        erelia::server_a::Server server(cfg);
        server.start();
        while (erelia::runtime::running)
        {
            server.dispatch();
            std::this_thread::sleep_for(std::chrono::milliseconds(5));
        }
        server.stop();
        return 0;
    }
    catch (const std::exception &e)
    {
        std::cerr << e.what() << '\n';
        SPK_LOG(Error) << "SERVER_A_ERROR " << e.what() << std::endl;
        return 1;
    }
}
