#include "erelia/core/configuration.hpp"
#include "erelia/server/processes.hpp"

#include <exception>
#include <iostream>

int main(int argc, char **argv)
{
    try
    {
        const auto args = erelia::core::parseConfigurationArguments(argc, argv);
        const auto configuration = erelia::core::loadRemoteNodeConfiguration(args.file);
        if (args.validateOnly)
        {
            return 0;
        }
        return erelia::server::runRemoteNodePlaceholder(configuration);
    }
    catch (const std::exception &error)
    {
        std::cerr << "EreliaRemoteNode: " << error.what() << '\n';
        return 1;
    }
}
