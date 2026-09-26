add_executable(EreliaIntegrationTestSuite
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
