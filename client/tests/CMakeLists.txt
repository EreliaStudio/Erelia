add_executable(EreliaClientTestSuite
    status_test.cpp
    golden_image_test.cpp
)
target_compile_features(EreliaClientTestSuite PRIVATE cxx_std_23)
target_compile_definitions(EreliaClientTestSuite PRIVATE
    ERELIA_CLIENT_TEST_RESOURCES_DIR="${CMAKE_CURRENT_SOURCE_DIR}/resources"
    ERELIA_CLIENT_TEST_RESULTS_DIR="${CMAKE_CURRENT_BINARY_DIR}/results"
)
target_link_libraries(EreliaClientTestSuite
    PRIVATE
        EreliaClientLibrary
        Sparkle::TestLibrary
        GTest::gtest_main
)
add_test(NAME EreliaClientTestSuite COMMAND EreliaClientTestSuite)
set_tests_properties(EreliaClientTestSuite PROPERTIES LABELS "client;graphics")

add_test(NAME EreliaClientSmoke COMMAND EreliaClient)
set_tests_properties(EreliaClientSmoke PROPERTIES LABELS "client;graphics")
