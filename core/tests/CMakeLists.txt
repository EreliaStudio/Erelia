add_executable(EreliaCoreTestSuite
    json_catalog_test.cpp
    status_test.cpp
    terrain_coordinate_test.cpp
    voxel_catalog_test.cpp
    voxel_cell_test.cpp
    voxel_definition_test.cpp
    voxel_shape_test.cpp
    voxel_volume_message_test.cpp
    voxel_volume_test.cpp
)
target_link_libraries(EreliaCoreTestSuite PRIVATE EreliaCore GTest::gtest_main)
target_compile_definitions(EreliaCoreTestSuite PRIVATE ERELIA_SOURCE_DIR="${PROJECT_SOURCE_DIR}")
add_test(NAME EreliaCoreTestSuite COMMAND EreliaCoreTestSuite)
set_tests_properties(EreliaCoreTestSuite PROPERTIES LABELS "core;headless")
