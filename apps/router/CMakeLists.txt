add_library(EreliaRouter STATIC src/router.cpp)
target_include_directories(EreliaRouter PUBLIC include)
target_link_libraries(EreliaRouter PUBLIC EreliaShared)
add_executable(EreliaRouterApp main.cpp)
target_link_libraries(EreliaRouterApp PRIVATE EreliaRouter)
set_target_properties(EreliaRouterApp PROPERTIES OUTPUT_NAME "Erelia-Router")
add_custom_command(TARGET EreliaRouterApp POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy_if_different "${PROJECT_SOURCE_DIR}/config/router.json" "$<TARGET_FILE_DIR:EreliaRouterApp>/router.json")
