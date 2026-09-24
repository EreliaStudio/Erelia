add_library(EreliaClientLibrary STATIC src/status.cpp)
target_compile_features(EreliaClientLibrary PUBLIC cxx_std_23)
target_include_directories(EreliaClientLibrary PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/include)
target_link_libraries(EreliaClientLibrary PUBLIC EreliaCore PRIVATE sparkle::sparkle)

add_executable(EreliaClient src/main.cpp)
target_link_libraries(EreliaClient PRIVATE EreliaClientLibrary)

if(BUILD_TESTING)
    add_subdirectory(tests)
endif()
