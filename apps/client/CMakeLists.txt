file(GLOB ERELIA_CLIENT_MAIN_SOURCES CONFIGURE_DEPENDS "${CMAKE_CURRENT_SOURCE_DIR}/*.cpp")
add_executable(EreliaClient ${ERELIA_CLIENT_MAIN_SOURCES})
target_link_libraries(EreliaClient PRIVATE erelia::client)

# Place an example config beside the executable on the first build. Subsequent
# builds must not overwrite local edits to the instance configuration.
add_custom_command(TARGET EreliaClient POST_BUILD
    COMMAND ${CMAKE_COMMAND}
        "-DSOURCE_CONFIG=${PROJECT_SOURCE_DIR}/config/examples/client.json"
        "-DTARGET_CONFIG=$<TARGET_FILE_DIR:EreliaClient>/client.json"
        -P "${PROJECT_SOURCE_DIR}/cmake/CopyConfigIfMissing.cmake"
    VERBATIM)
