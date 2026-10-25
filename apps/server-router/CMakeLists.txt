file(GLOB_RECURSE ERELIA_ROUTER_SOURCES CONFIGURE_DEPENDS
    "${CMAKE_CURRENT_SOURCE_DIR}/src/*.cpp")

add_library(EreliaRouterLibrary STATIC ${ERELIA_ROUTER_SOURCES})
target_include_directories(EreliaRouterLibrary PUBLIC "${CMAKE_CURRENT_SOURCE_DIR}/include")
target_link_libraries(EreliaRouterLibrary PUBLIC erelia::server)
target_compile_features(EreliaRouterLibrary PUBLIC cxx_std_23)
add_library(erelia::router ALIAS EreliaRouterLibrary)

add_executable(EreliaServerRouter main.cpp)
target_link_libraries(EreliaServerRouter PRIVATE erelia::router)
set_target_properties(EreliaServerRouter PROPERTIES OUTPUT_NAME "Erelia - Router")

# Tests live with this executable's library and are discovered automatically.
if(BUILD_TESTING)
    file(GLOB ERELIA_APP_TEST_SOURCES CONFIGURE_DEPENDS
        "${CMAKE_CURRENT_SOURCE_DIR}/tests/*.cpp")
    if(ERELIA_APP_TEST_SOURCES)
        find_package(GTest CONFIG REQUIRED)
        include(GoogleTest)
        add_executable(EreliaRouterTests ${ERELIA_APP_TEST_SOURCES})
        target_link_libraries(EreliaRouterTests PRIVATE GTest::gtest_main EreliaRouterLibrary)
        gtest_discover_tests(EreliaRouterTests
            TEST_PREFIX "Router."
            PROPERTIES LABELS "Router")
    endif()
endif()
