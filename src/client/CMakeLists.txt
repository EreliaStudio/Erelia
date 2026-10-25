file(GLOB_RECURSE ERELIA_CLIENT_SOURCES CONFIGURE_DEPENDS
    "${CMAKE_CURRENT_SOURCE_DIR}/src/*.cpp")
add_library(EreliaClientLibrary STATIC ${ERELIA_CLIENT_SOURCES})
target_include_directories(EreliaClientLibrary PUBLIC "${CMAKE_CURRENT_SOURCE_DIR}/include")
target_link_libraries(EreliaClientLibrary PUBLIC erelia::core erelia::sparkle_addon sparkle::sparkle)
target_compile_features(EreliaClientLibrary PUBLIC cxx_std_23)
add_library(erelia::client ALIAS EreliaClientLibrary)
