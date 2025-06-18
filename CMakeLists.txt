cmake_minimum_required(VERSION 3.16)
set(ProjectName "Erelia")
project(${ProjectName})

find_package(GLEW REQUIRED)
find_package(Sparkle REQUIRED)

set(INCLUDE_DIR ${CMAKE_CURRENT_SOURCE_DIR}/includes)
set(SRC_DIR ${CMAKE_CURRENT_SOURCE_DIR}/src)

file(GLOB_RECURSE HEADERS
    ${INCLUDE_DIR}/*.hpp
    ${INCLUDE_DIR}/*.h
)
file(GLOB_RECURSE SOURCES ${SRC_DIR}/*.cpp)

add_executable(${ProjectName} ${SOURCES} ${HEADERS})

target_link_libraries(${ProjectName} PRIVATE Sparkle::Sparkle)

target_include_directories(${ProjectName}
    PRIVATE
        ${INCLUDE_DIR}
        ${SPARKLE_INCLUDE_DIR}
)

set_target_properties(${ProjectName} PROPERTIES
    RUNTIME_OUTPUT_DIRECTORY ${CMAKE_SOURCE_DIR}/out/bin/$<IF:$<CONFIG:Debug>,Debug,Release>/${ARCH_FOLDER}
)
