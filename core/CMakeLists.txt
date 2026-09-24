add_library(EreliaCore STATIC src/status.cpp)
target_compile_features(EreliaCore PUBLIC cxx_std_23)
target_include_directories(EreliaCore PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/include)
target_link_libraries(EreliaCore PUBLIC sparkle::core)

if(BUILD_TESTING)
    add_subdirectory(tests)
endif()
