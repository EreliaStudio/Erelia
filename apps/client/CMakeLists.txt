add_library(EreliaClientLibrary STATIC src/ping_controller.cpp src/commands.cpp src/command_socket.cpp src/translator_service.cpp src/application.cpp)
target_include_directories(EreliaClientLibrary PUBLIC include)
target_link_libraries(EreliaClientLibrary PUBLIC EreliaShared sparkle::sparkle ws2_32)
add_executable(EreliaClientApp main.cpp)
target_link_libraries(EreliaClientApp PRIVATE EreliaClientLibrary)
set_target_properties(EreliaClientApp PROPERTIES OUTPUT_NAME "Erelia-Client")
add_custom_command(TARGET EreliaClientApp POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy_if_different "${PROJECT_SOURCE_DIR}/config/client.json" "$<TARGET_FILE_DIR:EreliaClientApp>/client.json"
    COMMAND ${CMAKE_COMMAND} -E make_directory "$<TARGET_FILE_DIR:EreliaClientApp>/translations"
    COMMAND ${CMAKE_COMMAND} -E copy_if_different "${PROJECT_SOURCE_DIR}/translations/en.i18l" "$<TARGET_FILE_DIR:EreliaClientApp>/translations/en.i18l")
