add_library(EreliaServerA STATIC src/server.cpp)
target_include_directories(EreliaServerA PUBLIC include)
target_link_libraries(EreliaServerA PUBLIC EreliaShared)
add_executable(EreliaServerAApp main.cpp)
target_link_libraries(EreliaServerAApp PRIVATE EreliaServerA)
set_target_properties(EreliaServerAApp PROPERTIES OUTPUT_NAME "Erelia-Server-A")
add_custom_command(TARGET EreliaServerAApp POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy_if_different "${PROJECT_SOURCE_DIR}/config/server-a.json" "$<TARGET_FILE_DIR:EreliaServerAApp>/server-a.json")
