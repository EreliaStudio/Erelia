add_executable(EreliaIntegrationTestSuite
    client_server_connection_test.cpp
    client_server_chunk_request_test.cpp
)
target_compile_features(EreliaIntegrationTestSuite PRIVATE cxx_std_23)
target_link_libraries(
    EreliaIntegrationTestSuite
    PRIVATE
        EreliaClientLibrary
        EreliaServerLibrary
        EreliaTerrainNodeLibrary
        GTest::gtest_main
)
add_dependencies(
    EreliaIntegrationTestSuite
    EreliaClient
    EreliaServer
)

add_test(
    NAME EreliaIntegrationTestSuite
    COMMAND EreliaIntegrationTestSuite
)
set_tests_properties(
    EreliaIntegrationTestSuite
    PROPERTIES
        LABELS "integration;client;server;terrain"
        TIMEOUT 30
)

if(WIN32)
    add_test(
        NAME EreliaClientServerProcessSmoke
        COMMAND powershell
            -NoProfile
            -ExecutionPolicy Bypass
            -File ${CMAKE_CURRENT_SOURCE_DIR}/client_server_connection_process_test.ps1
            -ServerExecutable $<TARGET_FILE:EreliaServer>
            -ClientExecutable $<TARGET_FILE:EreliaClient>
    )
    set_tests_properties(
        EreliaClientServerProcessSmoke
        PROPERTIES
            LABELS "integration;client;server;process"
            TIMEOUT 20
    )
endif()
