add_library(EreliaCore STATIC
    src/status.cpp
    src/structure/system/argument_parser.cpp
    src/terrain/chunk.cpp
    src/terrain/chunk_builder.cpp
    src/terrain/chunk_collection.cpp
    src/terrain/coordinate.cpp
    src/voxel/cell.cpp
    src/voxel/catalog.cpp
    src/voxel/definition.cpp
    src/voxel/definition_catalog.cpp
    src/voxel/shape.cpp
    src/voxel/shape_catalog.cpp
    src/voxel/volume.cpp
    src/voxel/volume_networking.cpp
    src/voxel/volume_buffer_pool.cpp
    src/voxel/volume_builder.cpp
)
target_compile_features(EreliaCore PUBLIC cxx_std_23)
target_include_directories(EreliaCore PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/include)
target_link_libraries(EreliaCore PUBLIC sparkle::core)

if(CMAKE_SYSTEM_NAME STREQUAL "Linux")
    target_link_libraries(EreliaCore PUBLIC atomic)
endif()

if(BUILD_TESTING)
    add_subdirectory(tests)
endif()
