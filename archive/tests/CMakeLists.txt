find_package(sparkle CONFIG REQUIRED COMPONENTS TestLibrary)
find_package(GTest CONFIG REQUIRED)

set(PLAYGROUND_TEST_SOURCE_DIR "${CMAKE_CURRENT_SOURCE_DIR}")
set(PLAYGROUND_TEST_LIBRARY_DIR "${PLAYGROUND_TEST_SOURCE_DIR}/library")

file(GLOB_RECURSE PLAYGROUND_TEST_SOURCES
    CONFIGURE_DEPENDS
    "${PLAYGROUND_TEST_SOURCE_DIR}/*.cpp"
)

file(GLOB_RECURSE PLAYGROUND_TEST_HEADERS
    CONFIGURE_DEPENDS
    "${PLAYGROUND_TEST_SOURCE_DIR}/*.hpp"
    "${PLAYGROUND_TEST_SOURCE_DIR}/*.h"
)

file(GLOB_RECURSE PLAYGROUND_TEST_LIBRARY_SOURCES
    CONFIGURE_DEPENDS
    "${PLAYGROUND_TEST_LIBRARY_DIR}/*.cpp"
)

file(GLOB_RECURSE PLAYGROUND_TEST_LIBRARY_HEADERS
    CONFIGURE_DEPENDS
    "${PLAYGROUND_TEST_LIBRARY_DIR}/*.hpp"
    "${PLAYGROUND_TEST_LIBRARY_DIR}/*.h"
)

set(PLAYGROUND_TEST_CASE_SOURCES ${PLAYGROUND_TEST_SOURCES})
list(REMOVE_ITEM PLAYGROUND_TEST_CASE_SOURCES ${PLAYGROUND_TEST_LIBRARY_SOURCES})
set(PLAYGROUND_TEST_CASE_HEADERS ${PLAYGROUND_TEST_HEADERS})
list(REMOVE_ITEM PLAYGROUND_TEST_CASE_HEADERS ${PLAYGROUND_TEST_LIBRARY_HEADERS})

set(PLAYGROUND_TEST_MAIN_SOURCE "${PLAYGROUND_TEST_SOURCE_DIR}/main.cpp")
list(REMOVE_ITEM PLAYGROUND_TEST_CASE_SOURCES "${PLAYGROUND_TEST_MAIN_SOURCE}")

if(NOT WIN32)
    list(FILTER PLAYGROUND_TEST_CASE_SOURCES EXCLUDE REGEX "/chunk_golden_tests\\.cpp$")
    list(FILTER PLAYGROUND_TEST_LIBRARY_SOURCES EXCLUDE REGEX "/current_chunk_golden_[^/]+\\.cpp$")
    list(FILTER PLAYGROUND_TEST_LIBRARY_SOURCES EXCLUDE REGEX "/palette_golden_[^/]+\\.cpp$")
endif()

#
# Playground Test Library
#

add_library(PlaygroundTestLibrary STATIC
    ${PLAYGROUND_TEST_LIBRARY_SOURCES}
    ${PLAYGROUND_TEST_LIBRARY_HEADERS}
)

target_compile_features(PlaygroundTestLibrary PUBLIC cxx_std_23)

target_include_directories(PlaygroundTestLibrary PUBLIC
    ${PLAYGROUND_TEST_LIBRARY_DIR}
)

target_link_libraries(PlaygroundTestLibrary PUBLIC
    PlaygroundCore
)

target_link_libraries(PlaygroundTestLibrary PRIVATE
    Sparkle::TestLibrary
)

target_compile_definitions(PlaygroundTestLibrary PRIVATE
    VOXEL_TEST_RESOURCE_DIR="${PROJECT_SOURCE_DIR}/resources"
    PLAYGROUND_TEST_RESOURCE_DIR="${PLAYGROUND_TEST_SOURCE_DIR}/resources"
    PLAYGROUND_TEST_RESULT_DIR="${CMAKE_CURRENT_BINARY_DIR}/results"
    PLAYGROUND_TEST_EXPECTED_CANDIDATE_DIR="${CMAKE_CURRENT_BINARY_DIR}/expectedImages"
)

set_target_properties(PlaygroundTestLibrary PROPERTIES
    CXX_EXTENSIONS OFF
)

configure_debug_symbols(PlaygroundTestLibrary)

#
# Playground Test Executable
#

add_executable(PlaygroundTest
    ${PLAYGROUND_TEST_CASE_SOURCES}
    ${PLAYGROUND_TEST_CASE_HEADERS}
)

if(EXISTS "${PLAYGROUND_TEST_MAIN_SOURCE}")
    target_sources(PlaygroundTest PRIVATE "${PLAYGROUND_TEST_MAIN_SOURCE}")
endif()

target_compile_features(PlaygroundTest PRIVATE cxx_std_23)

target_link_libraries(PlaygroundTest PRIVATE
    PlaygroundTestLibrary
    GTest::gtest
)

if(NOT EXISTS "${PLAYGROUND_TEST_MAIN_SOURCE}")
    target_link_libraries(PlaygroundTest PRIVATE GTest::gtest_main)
endif()

set_target_properties(PlaygroundTest PROPERTIES
    CXX_EXTENSIONS OFF
)

configure_debug_symbols(PlaygroundTest)

add_test(
    NAME playground_cpu_tests
    COMMAND PlaygroundTest --gtest_filter=-CurrentChunkGoldenTest.*
)

set_tests_properties(playground_cpu_tests PROPERTIES
    LABELS "cpu;headless"
    WORKING_DIRECTORY "${PROJECT_SOURCE_DIR}"
)

if(WIN32)
    add_test(
        NAME playground_gpu_tests
        COMMAND PlaygroundTest --gtest_filter=CurrentChunkGoldenTest.*
    )

    set_tests_properties(playground_gpu_tests PROPERTIES
        LABELS "gpu;golden;windows-opengl"
        WORKING_DIRECTORY "${PROJECT_SOURCE_DIR}"
    )
endif()
