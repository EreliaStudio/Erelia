# Change only the role name after copying this directory to a new app.
set(ERELIA_NODE_ROLE ExampleNode)

file(GLOB ERELIA_NODE_SOURCES CONFIGURE_DEPENDS
    "${CMAKE_CURRENT_SOURCE_DIR}/*.cpp")

add_executable(Erelia${ERELIA_NODE_ROLE} ${ERELIA_NODE_SOURCES})
target_link_libraries(Erelia${ERELIA_NODE_ROLE} PRIVATE erelia::server)
set_target_properties(Erelia${ERELIA_NODE_ROLE} PROPERTIES
    OUTPUT_NAME "Erelia - ${ERELIA_NODE_ROLE}")
