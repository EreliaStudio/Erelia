#include "erelia/router/application.hpp"

#include <exception>
#include <iostream>

int main(int argc, char **argv)
{
    try
    {
        return erelia::router::runApplication(argc, argv);
    }
    catch (const std::exception &error)
    {
        std::cerr << "EreliaServerRouter: " << error.what() << '\n';
        return 1;
    }
}
