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
