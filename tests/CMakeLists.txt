find_package(GTest CONFIG REQUIRED)
include(GoogleTest)

# New *.cpp files are discovered by CMake's CONFIGURE_DEPENDS glob.
function(erelia_add_unit_suite target family)
    file(GLOB_RECURSE ERELIA_TEST_SOURCES CONFIGURE_DEPENDS
        "${CMAKE_CURRENT_SOURCE_DIR}/${family}/*.cpp")
    add_executable(${target} ${ERELIA_TEST_SOURCES})
    target_link_libraries(${target} PRIVATE GTest::gtest_main ${ARGN})
    target_compile_features(${target} PRIVATE cxx_std_23)
    gtest_discover_tests(${target}
        TEST_PREFIX "${family}."
        PROPERTIES LABELS "${family}")
endfunction()

erelia_add_unit_suite(EreliaCoreTests Core erelia::core)
erelia_add_unit_suite(EreliaClientTests Client erelia::client)
erelia_add_unit_suite(EreliaServerTests Server erelia::server)
erelia_add_unit_suite(EreliaSparkleAddonTests SparkleAddon erelia::sparkle_addon)
erelia_add_unit_suite(EreliaClientServerIntegrationTests ClientServerIntegration
    erelia::client erelia::server)

# Process smoke tests validate that all three executable artifacts launch and exit.
add_test(NAME Executable.Client COMMAND $<TARGET_FILE:EreliaClient> --smoke)
set_tests_properties(Executable.Client PROPERTIES LABELS "Client")
add_test(NAME Executable.Router COMMAND $<TARGET_FILE:EreliaServerRouter> --smoke)
set_tests_properties(Executable.Router PROPERTIES LABELS "Server")
add_test(NAME Executable.RemoteNode COMMAND $<TARGET_FILE:EreliaRemoteNode> --smoke)
set_tests_properties(Executable.RemoteNode PROPERTIES LABELS "Server")
