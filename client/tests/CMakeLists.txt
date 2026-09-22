add_executable(EreliaClientTestSuite status_test.cpp)
target_link_libraries(EreliaClientTestSuite PRIVATE EreliaClientLibrary GTest::gtest_main)
add_test(NAME EreliaClientTestSuite COMMAND EreliaClientTestSuite)
set_tests_properties(EreliaClientTestSuite PROPERTIES LABELS "client;graphics")

add_test(NAME EreliaClientSmoke COMMAND EreliaClient)
set_tests_properties(EreliaClientSmoke PROPERTIES LABELS "client;graphics")
