#include "erelia/core/configuration.hpp"
#include "erelia/server/processes.hpp"

#include <exception>
#include <iostream>

int main(int argc, char **argv)
{
    try
    {
        const auto args = erelia::core::parseConfigurationArguments(argc, argv);
        const auto configuration = erelia::core::loadServerRouterConfiguration(args.file);
        if (args.validateOnly)
        {
            return 0;
        }
        return erelia::server::runRouterPlaceholder(configuration);
    }
    catch (const std::exception &error)
    {
        std::cerr << "EreliaServerRouter: " << error.what() << '\n';
        return 1;
    }
}
