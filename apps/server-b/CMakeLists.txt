add_library(EreliaServerB STATIC src/server.cpp)
target_include_directories(EreliaServerB PUBLIC include)
target_link_libraries(EreliaServerB PUBLIC EreliaShared)
add_executable(EreliaServerBApp main.cpp)
target_link_libraries(EreliaServerBApp PRIVATE EreliaServerB)
set_target_properties(EreliaServerBApp PROPERTIES OUTPUT_NAME "Erelia-Server-B")
add_custom_command(TARGET EreliaServerBApp POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy_if_different "${PROJECT_SOURCE_DIR}/config/server-b.json" "$<TARGET_FILE_DIR:EreliaServerBApp>/server-b.json")
