add_library(EreliaClientLibrary STATIC
    src/event_center.cpp
    src/client_network_manager.cpp
    src/client_runtime.cpp
    src/terrain_collections.cpp
    src/terrain_streaming_behaviour.cpp
    src/client_configuration.cpp
    src/command/connect_command.cpp
    src/connection_manager.cpp
    src/console.cpp
    src/console_entry.cpp
    src/main_interface.cpp
    src/world_manager.cpp
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
add_custom_command(TARGET EreliaClient POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy_directory
        ${CMAKE_CURRENT_SOURCE_DIR}/resources/i18n
        $<TARGET_FILE_DIR:EreliaClient>/i18n
    VERBATIM
)
set_property(TARGET EreliaClient APPEND PROPERTY LINK_DEPENDS
    ${CMAKE_CURRENT_SOURCE_DIR}/resources/i18n/en.json
)

if(BUILD_TESTING)
    add_subdirectory(tests)
endif()
