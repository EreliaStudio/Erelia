#include "erelia/client/application.hpp"
#include <erelia/shared/runtime.hpp>
#include <iostream>

int main(int argc, char **argv)
{
    try
    {
        auto args = erelia::config::arguments(argc, argv, "client.json");
        auto cfg = erelia::config::client(args.file);
        if (args.validateOnly) return 0;
        auto log = erelia::runtime::logTo(cfg.logFile);
        return erelia::client::runApplication(cfg);
    }
    catch (const std::exception &e)
    {
        std::cerr << e.what() << '\n';
        SPK_LOG(Error) << "CLIENT_ERROR " << e.what() << std::endl;
        return 1;
    }
}
