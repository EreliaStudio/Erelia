add_executable(EreliaServerTestSuite
    router_test.cpp
    status_test.cpp
)
target_link_libraries(EreliaServerTestSuite PRIVATE EreliaServerLibrary GTest::gtest_main)
add_test(NAME EreliaServerTestSuite COMMAND EreliaServerTestSuite)
set_tests_properties(EreliaServerTestSuite PROPERTIES LABELS "server;headless")

add_test(NAME EreliaServerSmoke COMMAND EreliaServer --help)
set_tests_properties(EreliaServerSmoke PROPERTIES LABELS "server;headless")
