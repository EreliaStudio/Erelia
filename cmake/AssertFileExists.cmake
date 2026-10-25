if(NOT DEFINED FILE_TO_CHECK OR NOT EXISTS "${FILE_TO_CHECK}")
    message(FATAL_ERROR "Expected configuration file is missing: ${FILE_TO_CHECK}")
endif()
