add_library(Erelia__NODE_NAME__NodeLibrary STATIC
    src/__NODE_SNAKE___node.cpp
    src/__NODE_SNAKE___node_application.cpp
)
target_compile_features(Erelia__NODE_NAME__NodeLibrary PUBLIC cxx_std_23)
target_include_directories(
    Erelia__NODE_NAME__NodeLibrary
    PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/include
)
target_link_libraries(Erelia__NODE_NAME__NodeLibrary PUBLIC EreliaCore)

add_executable(Erelia__NODE_NAME__Node src/main.cpp)
target_link_libraries(Erelia__NODE_NAME__Node PRIVATE Erelia__NODE_NAME__NodeLibrary)
add_dependencies(EreliaServerNodes Erelia__NODE_NAME__Node)

if(BUILD_TESTING)
    target_link_libraries(EreliaServerTestSuite PRIVATE Erelia__NODE_NAME__NodeLibrary)
    add_subdirectory(tests)
endif()
