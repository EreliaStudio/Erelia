add_executable(EreliaServerTestSuite status_test.cpp)
target_link_libraries(EreliaServerTestSuite PRIVATE EreliaServerLibrary GTest::gtest_main)
add_test(NAME EreliaServerTestSuite COMMAND EreliaServerTestSuite)
set_tests_properties(EreliaServerTestSuite PROPERTIES LABELS "server;headless")

add_test(NAME EreliaServerSmoke COMMAND EreliaServer)
set_tests_properties(EreliaServerSmoke PROPERTIES LABELS "server;headless")
