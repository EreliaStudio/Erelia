add_library(EreliaCore STATIC
    src/status.cpp
    src/terrain/coordinate.cpp
    src/voxel/cell.cpp
    src/voxel/catalog.cpp
    src/voxel/definition.cpp
    src/voxel/shape.cpp
    src/voxel/volume.cpp
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
