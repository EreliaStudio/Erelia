#include "erelia/remote_node/application.hpp"

#include <exception>
#include <iostream>

int main(int argc, char **argv)
{
    try
    {
        return erelia::remote_node::runApplication(argc, argv);
    }
    catch (const std::exception &error)
    {
        std::cerr << "EreliaRemoteNode: " << error.what() << '\n';
        return 1;
    }
}
