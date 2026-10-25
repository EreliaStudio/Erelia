file(GLOB_RECURSE ERELIA_REMOTE_NODE_SOURCES CONFIGURE_DEPENDS
    "${CMAKE_CURRENT_SOURCE_DIR}/src/*.cpp")

add_library(EreliaRemoteNodeLibrary STATIC ${ERELIA_REMOTE_NODE_SOURCES})
target_include_directories(EreliaRemoteNodeLibrary PUBLIC "${CMAKE_CURRENT_SOURCE_DIR}/include")
target_link_libraries(EreliaRemoteNodeLibrary PUBLIC erelia::server)
target_compile_features(EreliaRemoteNodeLibrary PUBLIC cxx_std_23)
add_library(erelia::remote_node ALIAS EreliaRemoteNodeLibrary)

add_executable(EreliaRemoteNode main.cpp)
target_link_libraries(EreliaRemoteNode PRIVATE erelia::remote_node)
set_target_properties(EreliaRemoteNode PROPERTIES OUTPUT_NAME "Erelia - Server")
