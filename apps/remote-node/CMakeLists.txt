file(GLOB ERELIA_REMOTE_NODE_MAIN_SOURCES CONFIGURE_DEPENDS "${CMAKE_CURRENT_SOURCE_DIR}/*.cpp")
add_executable(EreliaRemoteNode ${ERELIA_REMOTE_NODE_MAIN_SOURCES})
target_link_libraries(EreliaRemoteNode PRIVATE erelia::server)

# Preserve the stable CMake target for tooling while naming the distributed binary.
set_target_properties(EreliaRemoteNode PROPERTIES OUTPUT_NAME "Erelia - Server")
