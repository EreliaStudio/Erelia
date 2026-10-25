cmake_minimum_required(VERSION 3.25)
project(Erelia VERSION 0.1.0 LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 23)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

include(CTest)

# On Linux the foundation builds against Sparkle Core only.
# Graphical Client and GUI tests are enabled on Windows.
option(ERELIA_BUILD_GRAPHICS "Build the graphical Erelia Client and tests" ${WIN32})
if(ERELIA_BUILD_GRAPHICS)
    find_package(sparkle CONFIG REQUIRED COMPONENTS Core Graphics)
else()
    find_package(sparkle CONFIG REQUIRED COMPONENTS Core)
endif()

# Each module enumerates its own sources. The root stays unchanged as new sources are added.
add_subdirectory(src/core)
add_subdirectory(src/sparkle_addon)
if(ERELIA_BUILD_GRAPHICS)
    add_subdirectory(src/client)
endif()
add_subdirectory(src/server)
add_subdirectory(apps)

if(BUILD_TESTING)
    add_subdirectory(tests)
endif()
