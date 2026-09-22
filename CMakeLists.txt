cmake_minimum_required(VERSION 3.25)
project(Erelia VERSION 0.1.0 LANGUAGES CXX)

option(ERELIA_BUILD_CLIENT "Build the graphical client" ON)
option(ERELIA_BUILD_SERVER "Build the headless server" ON)

include(CTest)

if(ERELIA_BUILD_CLIENT)
    set(ERELIA_SPARKLE_COMPONENTS Core Graphics)
    if(BUILD_TESTING)
        list(APPEND ERELIA_SPARKLE_COMPONENTS TestLibrary)
    endif()
    find_package(sparkle 0.1.3 CONFIG REQUIRED COMPONENTS ${ERELIA_SPARKLE_COMPONENTS})
else()
    find_package(sparkle 0.1.3 CONFIG REQUIRED COMPONENTS Core)
endif()

if(BUILD_TESTING)
    find_package(GTest CONFIG REQUIRED)
endif()

add_subdirectory(core)

if(ERELIA_BUILD_SERVER)
    add_subdirectory(server)
endif()

if(ERELIA_BUILD_CLIENT)
    add_subdirectory(client)
endif()
