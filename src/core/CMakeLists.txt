file(GLOB_RECURSE ERELIA_CORE_SOURCES CONFIGURE_DEPENDS
    "${CMAKE_CURRENT_SOURCE_DIR}/src/*.cpp")
add_library(EreliaCore STATIC ${ERELIA_CORE_SOURCES})
target_include_directories(EreliaCore PUBLIC "${CMAKE_CURRENT_SOURCE_DIR}/include")
target_link_libraries(EreliaCore PUBLIC sparkle::core)
target_compile_features(EreliaCore PUBLIC cxx_std_23)
add_library(erelia::core ALIAS EreliaCore)
