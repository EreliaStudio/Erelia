file(GLOB_RECURSE ERELIA_ADDON_SOURCES CONFIGURE_DEPENDS
    "${CMAKE_CURRENT_SOURCE_DIR}/src/*.cpp")
add_library(EreliaSparkleAddon STATIC ${ERELIA_ADDON_SOURCES})
target_include_directories(EreliaSparkleAddon PUBLIC "${CMAKE_CURRENT_SOURCE_DIR}/include")
target_link_libraries(EreliaSparkleAddon PUBLIC erelia::core)
target_compile_features(EreliaSparkleAddon PUBLIC cxx_std_23)
add_library(erelia::sparkle_addon ALIAS EreliaSparkleAddon)
