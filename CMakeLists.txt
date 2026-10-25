cmake_minimum_required(VERSION 3.25)
project(Erelia VERSION 0.1.0 LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 23)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

include(CTest)
find_package(sparkle CONFIG REQUIRED COMPONENTS Core Graphics)

# Each module enumerates its own sources. The root stays unchanged as new sources are added.
add_subdirectory(src/core)
add_subdirectory(src/sparkle_addon)
add_subdirectory(src/client)
add_subdirectory(src/server)
add_subdirectory(apps)

if(BUILD_TESTING)
    add_subdirectory(tests)
endif()
