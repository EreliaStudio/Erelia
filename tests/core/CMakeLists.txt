add_executable(EreliaCoreTestSuite status_test.cpp)
target_link_libraries(EreliaCoreTestSuite PRIVATE EreliaCore GTest::gtest_main)
add_test(NAME EreliaCoreTestSuite COMMAND EreliaCoreTestSuite)
set_tests_properties(EreliaCoreTestSuite PROPERTIES LABELS "core;headless")
