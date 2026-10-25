find_package(GTest CONFIG REQUIRED)
include(GoogleTest)

# New *.cpp files are discovered by CMake's CONFIGURE_DEPENDS glob.
function(erelia_add_unit_suite target family label)
    file(GLOB_RECURSE ERELIA_TEST_SOURCES CONFIGURE_DEPENDS
        "${CMAKE_CURRENT_SOURCE_DIR}/${family}/*.cpp")
    add_executable(${target} ${ERELIA_TEST_SOURCES})
    target_link_libraries(${target} PRIVATE GTest::gtest_main ${ARGN})
    target_compile_features(${target} PRIVATE cxx_std_23)
    gtest_discover_tests(${target}
        TEST_PREFIX "${family}."
        PROPERTIES LABELS "${label}")
endfunction()

erelia_add_unit_suite(EreliaCoreTests Core Core erelia::core)
target_compile_definitions(EreliaCoreTests PRIVATE
    ERELIA_CONFIG_EXAMPLES_DIR="${PROJECT_SOURCE_DIR}/config/examples"
    ERELIA_CONFIG_FIXTURES_DIR="${CMAKE_CURRENT_SOURCE_DIR}/fixtures/config")

if(ERELIA_BUILD_GRAPHICS)
    erelia_add_unit_suite(EreliaClientTests Client Client erelia::client)
endif()
erelia_add_unit_suite(EreliaServerTests Server Server erelia::server)
erelia_add_unit_suite(EreliaSparkleAddonTests SparkleAddon SparkleAddonCore erelia::sparkle_addon)

# Future graphics-addon tests live in their own directory. Do not manufacture
# a passing suite while there are no graphics-addon tests to execute.
file(GLOB_RECURSE ERELIA_GRAPHICS_ADDON_TEST_SOURCES CONFIGURE_DEPENDS
    "${CMAKE_CURRENT_SOURCE_DIR}/SparkleAddonGraphics/*.cpp")
if(ERELIA_BUILD_GRAPHICS AND ERELIA_GRAPHICS_ADDON_TEST_SOURCES)
    erelia_add_unit_suite(EreliaSparkleAddonGraphicsTests
        SparkleAddonGraphics SparkleAddonGraphics erelia::sparkle_addon sparkle::sparkle)
endif()
if(ERELIA_BUILD_GRAPHICS)
    erelia_add_unit_suite(EreliaClientServerIntegrationTests ClientServerIntegration ClientServerIntegration
        erelia::client erelia::router erelia::remote_node)
    target_compile_definitions(EreliaClientServerIntegrationTests PRIVATE ERELIA_WITH_CLIENT)
else()
    erelia_add_unit_suite(EreliaClientServerIntegrationTests ClientServerIntegration ClientServerIntegration
        erelia::router erelia::remote_node)
endif()

# The Client starts with no arguments. Check the packaged config without
# launching a graphical window on a headless CTest machine.
if(ERELIA_BUILD_GRAPHICS)
    add_test(NAME Executable.Client.ConfigInstalled
    COMMAND ${CMAKE_COMMAND}
        "-DFILE_TO_CHECK=$<TARGET_FILE_DIR:EreliaClient>/client.json"
        -P "${PROJECT_SOURCE_DIR}/cmake/AssertFileExists.cmake")
    set_tests_properties(Executable.Client.ConfigInstalled PROPERTIES LABELS "Client")
endif()

# Server processes accept instance-specific configuration files.
add_test(NAME Executable.Router.Config
    COMMAND $<TARGET_FILE:EreliaServerRouter>
        --validate-config "${PROJECT_SOURCE_DIR}/config/examples/router.json")
set_tests_properties(Executable.Router.Config PROPERTIES LABELS "Router")
add_test(NAME Executable.RemoteNode.Config
    COMMAND $<TARGET_FILE:EreliaRemoteNode>
        --validate-config "${PROJECT_SOURCE_DIR}/config/examples/world-node.json")
set_tests_properties(Executable.RemoteNode.Config PROPERTIES LABELS "Server;RemoteNode")
add_test(NAME Executable.SecondRemoteNode.Config
    COMMAND $<TARGET_FILE:EreliaRemoteNode>
        --validate-config "${PROJECT_SOURCE_DIR}/config/examples/chat-node.json")
set_tests_properties(Executable.SecondRemoteNode.Config PROPERTIES LABELS "RemoteNode")

# A copied example node must build as its own executable without central
# source-file registration or a separate CI workflow.
add_test(NAME Executable.ExampleRemoteNode.Config
    COMMAND $<TARGET_FILE:EreliaExampleNode>
        --validate-config "${PROJECT_SOURCE_DIR}/config/examples/example-node.json")
set_tests_properties(Executable.ExampleRemoteNode.Config PROPERTIES LABELS "Server;ExampleNode")
