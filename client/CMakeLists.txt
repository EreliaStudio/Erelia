add_library(EreliaClientLibrary STATIC
    src/application.cpp
    src/connection_manager.cpp
    src/console.cpp
    src/console_entry.cpp
    src/main_application_widget.cpp
    src/service.cpp
)
target_compile_features(EreliaClientLibrary PUBLIC cxx_std_23)
target_include_directories(EreliaClientLibrary PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/include)
target_link_libraries(
    EreliaClientLibrary
    PUBLIC
        EreliaCore
        sparkle::core
        sparkle::sparkle
)

add_executable(EreliaClient src/main.cpp)
target_link_libraries(EreliaClient PRIVATE EreliaClientLibrary)

if(BUILD_TESTING)
    add_subdirectory(tests)
endif()
