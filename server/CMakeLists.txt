add_library(EreliaServerLibrary STATIC
    src/application.cpp
    src/router.cpp
    src/status.cpp
)
target_compile_features(EreliaServerLibrary PUBLIC cxx_std_23)
target_include_directories(EreliaServerLibrary PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/include)
target_link_libraries(EreliaServerLibrary PUBLIC EreliaCore)

add_executable(EreliaServer src/main.cpp)
target_link_libraries(EreliaServer PRIVATE EreliaServerLibrary)

add_custom_target(EreliaServerNodes)

if(BUILD_TESTING)
    add_subdirectory(tests)
endif()

file(
    GLOB nodeDirectories
    CONFIGURE_DEPENDS
    RELATIVE "${CMAKE_CURRENT_SOURCE_DIR}/nodes"
    "${CMAKE_CURRENT_SOURCE_DIR}/nodes/*"
)

foreach(nodeDirectory IN LISTS nodeDirectories)
    if(
        IS_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/nodes/${nodeDirectory}"
        AND EXISTS "${CMAKE_CURRENT_SOURCE_DIR}/nodes/${nodeDirectory}/CMakeLists.txt"
    )
        add_subdirectory("nodes/${nodeDirectory}")
    endif()
endforeach()
