cmake_minimum_required(VERSION 3.21)

project(DivitageLinuxApp VERSION 0.1.0 LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

find_package(Qt6 REQUIRED COMPONENTS Widgets)
qt_standard_project_setup()

set(SOURCES
    src/main.cpp
    src/MainWindow.cpp
    src/AppState.cpp
    src/ConverterPanel.cpp
    src/DropArea.cpp
    src/SettingsPanel.cpp
    src/HowToPanel.cpp
    src/ConversionWorker.cpp
)

set(HEADERS
    include/MainWindow.h
    include/AppState.h
    include/ConverterPanel.h
    include/DropArea.h
    include/SettingsPanel.h
    include/HowToPanel.h
    include/ConversionWorker.h
)

qt_add_executable(DivitageLinuxApp
    ${SOURCES}
    ${HEADERS}
)

target_include_directories(DivitageLinuxApp PRIVATE include)
target_link_libraries(DivitageLinuxApp PRIVATE Qt6::Widgets)

# Enable testing
enable_testing()

# Find Qt Test module
find_package(Qt6 COMPONENTS Test QUIET)

if(Qt6Test_FOUND)
    # Create a library with the core logic (excluding main.cpp)
    set(LIB_SOURCES
        src/MainWindow.cpp
        src/AppState.cpp
        src/ConverterPanel.cpp
        src/DropArea.cpp
        src/SettingsPanel.cpp
        src/HowToPanel.cpp
        src/ConversionWorker.cpp
    )

    add_library(DivitageLinuxCore STATIC ${LIB_SOURCES} ${HEADERS})
    target_include_directories(DivitageLinuxCore PUBLIC include)
    target_link_libraries(DivitageLinuxCore PUBLIC Qt6::Widgets)

    # Link the executable to the core library
    target_link_libraries(DivitageLinuxApp PRIVATE DivitageLinuxCore)

    # Add test subdirectory
    add_subdirectory(tests)
else()
    message(WARNING "Qt6::Test not found - tests will not be built")
endif()
