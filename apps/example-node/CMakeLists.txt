# A copied server role changes only this value and its request handler.
set(ERELIA_NODE_ROLE ExampleNode)

file(GLOB_RECURSE ERELIA_NODE_SOURCES CONFIGURE_DEPENDS
    "${CMAKE_CURRENT_SOURCE_DIR}/src/*.cpp")
add_library(Erelia${ERELIA_NODE_ROLE}Library STATIC ${ERELIA_NODE_SOURCES})
target_include_directories(Erelia${ERELIA_NODE_ROLE}Library PUBLIC "${CMAKE_CURRENT_SOURCE_DIR}/include")
target_link_libraries(Erelia${ERELIA_NODE_ROLE}Library PUBLIC erelia::server)
target_compile_features(Erelia${ERELIA_NODE_ROLE}Library PUBLIC cxx_std_23)
# Only the template's own tests refer to this alias; cloned nodes get their own target.
if(ERELIA_NODE_ROLE STREQUAL "ExampleNode")
    add_library(erelia::example_node ALIAS Erelia${ERELIA_NODE_ROLE}Library)
endif()

add_executable(Erelia${ERELIA_NODE_ROLE} main.cpp)
target_link_libraries(Erelia${ERELIA_NODE_ROLE} PRIVATE Erelia${ERELIA_NODE_ROLE}Library)
set_target_properties(Erelia${ERELIA_NODE_ROLE} PROPERTIES
    OUTPUT_NAME "Erelia - ${ERELIA_NODE_ROLE}")

# Tests live with this executable's library and are discovered automatically.
if(BUILD_TESTING)
    file(GLOB ERELIA_APP_TEST_SOURCES CONFIGURE_DEPENDS
        "${CMAKE_CURRENT_SOURCE_DIR}/tests/*.cpp")
    if(ERELIA_APP_TEST_SOURCES)
        find_package(GTest CONFIG REQUIRED)
        include(GoogleTest)
        add_executable(Erelia${ERELIA_NODE_ROLE}Tests ${ERELIA_APP_TEST_SOURCES})
        target_link_libraries(Erelia${ERELIA_NODE_ROLE}Tests PRIVATE GTest::gtest_main Erelia${ERELIA_NODE_ROLE}Library)
        gtest_discover_tests(Erelia${ERELIA_NODE_ROLE}Tests
            TEST_PREFIX "${ERELIA_NODE_ROLE}."
            PROPERTIES LABELS "Server;${ERELIA_NODE_ROLE}")
    endif()
endif()
