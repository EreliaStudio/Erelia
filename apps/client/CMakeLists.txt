file(GLOB ERELIA_CLIENT_MAIN_SOURCES CONFIGURE_DEPENDS "${CMAKE_CURRENT_SOURCE_DIR}/*.cpp")
add_executable(EreliaClient ${ERELIA_CLIENT_MAIN_SOURCES})
target_link_libraries(EreliaClient PRIVATE erelia::client)
