file(GLOB ERELIA_EXAMPLE_NODE_SOURCES CONFIGURE_DEPENDS
    "${CMAKE_CURRENT_SOURCE_DIR}/*.cpp")

add_executable(EreliaExampleNode ${ERELIA_EXAMPLE_NODE_SOURCES})
target_link_libraries(EreliaExampleNode PRIVATE erelia::server)

# Preserve the stable CMake target for tooling while naming the distributed binary.
set_target_properties(EreliaExampleNode PROPERTIES OUTPUT_NAME "Erelia - ExampleNode")
