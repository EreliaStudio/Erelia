file(GLOB ERELIA_ROUTER_MAIN_SOURCES CONFIGURE_DEPENDS "${CMAKE_CURRENT_SOURCE_DIR}/*.cpp")
add_executable(EreliaServerRouter ${ERELIA_ROUTER_MAIN_SOURCES})
target_link_libraries(EreliaServerRouter PRIVATE erelia::server)
