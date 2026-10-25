file(GLOB_RECURSE ERELIA_ROUTER_SOURCES CONFIGURE_DEPENDS
    "${CMAKE_CURRENT_SOURCE_DIR}/src/*.cpp")

add_library(EreliaRouterLibrary STATIC ${ERELIA_ROUTER_SOURCES})
target_include_directories(EreliaRouterLibrary PUBLIC "${CMAKE_CURRENT_SOURCE_DIR}/include")
target_link_libraries(EreliaRouterLibrary PUBLIC erelia::server)
target_compile_features(EreliaRouterLibrary PUBLIC cxx_std_23)
add_library(erelia::router ALIAS EreliaRouterLibrary)

add_executable(EreliaServerRouter main.cpp)
target_link_libraries(EreliaServerRouter PRIVATE erelia::router)
set_target_properties(EreliaServerRouter PROPERTIES OUTPUT_NAME "Erelia - Router")
