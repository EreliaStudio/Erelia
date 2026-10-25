file(GLOB ERELIA_REMOTE_NODE_MAIN_SOURCES CONFIGURE_DEPENDS "${CMAKE_CURRENT_SOURCE_DIR}/*.cpp")
add_executable(EreliaRemoteNode ${ERELIA_REMOTE_NODE_MAIN_SOURCES})
target_link_libraries(EreliaRemoteNode PRIVATE erelia::server)
