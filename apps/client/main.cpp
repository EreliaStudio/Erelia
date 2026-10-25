#include "erelia/client/application.hpp"

#include <exception>
#include <iostream>

int main()
{
    try
    {
        return erelia::client::runApplication();
    }
    catch (const std::exception &error)
    {
        std::cerr << "EreliaClient: " << error.what() << '\n';
        return 1;
    }
}
