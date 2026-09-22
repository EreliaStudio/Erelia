add_subdirectory(core)

if(ERELIA_BUILD_SERVER)
    add_subdirectory(server)
endif()

if(ERELIA_BUILD_CLIENT)
    add_subdirectory(client)
endif()
