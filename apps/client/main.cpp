#include "erelia/client/application.hpp"
#include "erelia/core/configuration.hpp"

#include <exception>
#include <iostream>

int main(int argc, char **argv)
{
    try
    {
        const auto args = erelia::core::parseConfigurationArguments(argc, argv);
        const auto configuration = erelia::core::loadClientConfiguration(args.file);
        if (args.validateOnly)
        {
            return 0;
        }
        return erelia::client::runApplication(configuration);
    }
    catch (const std::exception &error)
    {
        std::cerr << "EreliaClient: " << error.what() << '\n';
        return 1;
    }
}
