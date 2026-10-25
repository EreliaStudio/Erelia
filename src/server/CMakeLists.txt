file(GLOB_RECURSE ERELIA_SERVER_SOURCES CONFIGURE_DEPENDS
    "${CMAKE_CURRENT_SOURCE_DIR}/src/*.cpp")
add_library(EreliaServerLibrary STATIC ${ERELIA_SERVER_SOURCES})
target_include_directories(EreliaServerLibrary PUBLIC "${CMAKE_CURRENT_SOURCE_DIR}/include")
target_link_libraries(EreliaServerLibrary PUBLIC erelia::core erelia::sparkle_addon)
target_compile_features(EreliaServerLibrary PUBLIC cxx_std_23)
add_library(erelia::server ALIAS EreliaServerLibrary)
