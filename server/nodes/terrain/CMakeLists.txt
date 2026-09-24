add_library(EreliaTerrainNodeLibrary STATIC
    src/terrain_node.cpp
    src/terrain_node_application.cpp
)
target_compile_features(EreliaTerrainNodeLibrary PUBLIC cxx_std_23)
target_include_directories(
    EreliaTerrainNodeLibrary
    PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/include
)
target_link_libraries(EreliaTerrainNodeLibrary PUBLIC EreliaServerLibrary)

add_executable(EreliaTerrainNode src/main.cpp)
target_link_libraries(EreliaTerrainNode PRIVATE EreliaTerrainNodeLibrary)
add_dependencies(EreliaServerNodes EreliaTerrainNode)

if(BUILD_TESTING)
    target_link_libraries(EreliaServerTestSuite PRIVATE EreliaTerrainNodeLibrary)
    add_subdirectory(tests)
endif()
