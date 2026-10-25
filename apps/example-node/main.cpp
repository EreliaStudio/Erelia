#include "erelia/node_app/application.hpp"

#include <exception>
#include <iostream>

int main(int argc, char **argv)
{
    try
    {
        return erelia::node_app::runApplication(argc, argv);
    }
    catch (const std::exception &error)
    {
        std::cerr << "EreliaExampleNode: " << error.what() << '\n';
        return 1;
    }
}
