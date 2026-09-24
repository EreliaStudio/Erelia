add_library(EreliaServerLibrary STATIC src/status.cpp)
target_compile_features(EreliaServerLibrary PUBLIC cxx_std_23)
target_include_directories(EreliaServerLibrary PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/include)
target_link_libraries(EreliaServerLibrary PUBLIC EreliaCore)

add_executable(EreliaServer src/main.cpp)
target_link_libraries(EreliaServer PRIVATE EreliaServerLibrary)

if(BUILD_TESTING)
    add_subdirectory(tests)
endif()
