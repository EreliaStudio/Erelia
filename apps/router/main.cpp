#include "erelia/router/router.hpp"
#include <erelia/shared/runtime.hpp>
#include <chrono>
#include <iostream>
#include <thread>

int main(int argc, char **argv)
{
    try
    {
        auto args = erelia::config::arguments(argc, argv, "router.json");
        auto configuration = erelia::config::router(args.file);
        if (args.validateOnly) return 0;
        auto log = erelia::runtime::logTo(configuration.logFile);
        erelia::runtime::signals();
        erelia::router::Router router(configuration);
        router.start();
        while (erelia::runtime::running)
        {
            router.dispatch();
            std::this_thread::sleep_for(std::chrono::milliseconds(5));
        }
        router.stop();
        return 0;
    }
    catch (const std::exception &e)
    {
        std::cerr << e.what() << '\n';
        SPK_LOG(Error) << "ROUTER_ERROR " << e.what() << std::endl;
        return 1;
    }
}
