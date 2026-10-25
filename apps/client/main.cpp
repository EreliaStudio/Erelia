#include "erelia/client/application.hpp"

#include <exception>
#include <iostream>
#include <string_view>

int main(int argc, char **argv)
{
    if (argc == 2 && std::string_view(argv[1]) == "--smoke")
    {
        return 0;
    }
    if (argc != 1)
    {
        std::cerr << "Usage: EreliaClient [--smoke]\n";
        return 2;
    }
    try
    {
        return erelia::client::runApplication();
    }
    catch (const std::exception &error)
    {
        std::cerr << error.what() << '\n';
        return 1;
    }
}
