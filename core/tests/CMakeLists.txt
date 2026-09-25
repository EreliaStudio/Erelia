add_executable(EreliaCoreTestSuite
    chunk_collection_test.cpp
    chunk_protocol_error_test.cpp
    chunk_protocol_request_test.cpp
    chunk_protocol_response_test.cpp
    chunk_test.cpp
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

add_custom_command(
    TARGET EreliaCoreTestSuite
    POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E rm -rf "${CMAKE_BINARY_DIR}/resources"
    COMMAND ${CMAKE_COMMAND} -E copy_directory
        "${PROJECT_SOURCE_DIR}/resources"
        "${CMAKE_BINARY_DIR}/resources"
    COMMENT "Copying Erelia test resources into the build directory"
)

add_test(NAME EreliaCoreTestSuite COMMAND EreliaCoreTestSuite)
set_tests_properties(
    EreliaCoreTestSuite
    PROPERTIES
        LABELS "core;headless"
        WORKING_DIRECTORY "${CMAKE_BINARY_DIR}"
)
