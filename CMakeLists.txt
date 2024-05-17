cmake_minimum_required(VERSION 3.22)
set(CMAKE_CXX_STANDARD 20)

set(ProductName "Erelia")

project(${ProductName}
	VERSION 1.0
	DESCRIPTION "Single player Tactical monster catching game."
	LANGUAGES CXX)

find_package(Sparkle REQUIRED)

file(GLOB_RECURSE SRC_FILES "${CMAKE_CURRENT_SOURCE_DIR}/src/*.cpp")
add_executable(${ProductName} ${SRC_FILES})

target_include_directories(${ProductName} PRIVATE
	"${CMAKE_CURRENT_SOURCE_DIR}/src"
    ${SPARKLE_INCLUDE_DIRS}
)

target_link_libraries(${ProductName} PUBLIC ${SPARKLE_LIBRARIES})

target_link_directories(${ProductName} PUBLIC ${SPARKLE_LIBRARY_DIRS})