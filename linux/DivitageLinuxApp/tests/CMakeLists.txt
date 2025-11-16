# Test CMake configuration
cmake_minimum_required(VERSION 3.21)

# Find Qt Test
find_package(Qt6 REQUIRED COMPONENTS Test)

# Enable automatic MOC, UIC, and RCC
set(CMAKE_AUTOMOC ON)

# Test for AppState
set(TEST_APPSTATE_SOURCES
    test_appstate.cpp
)

qt_add_executable(test_appstate ${TEST_APPSTATE_SOURCES})
target_link_libraries(test_appstate PRIVATE
    Qt6::Test
    Qt6::Widgets
    DivitageLinuxCore
)

# Add test to CTest
add_test(NAME AppStateTest COMMAND test_appstate)

# Test for file path utilities
set(TEST_FILEPATH_SOURCES
    test_filepath.cpp
)

qt_add_executable(test_filepath ${TEST_FILEPATH_SOURCES})
target_link_libraries(test_filepath PRIVATE
    Qt6::Test
    Qt6::Widgets
    DivitageLinuxCore
)

# Add test to CTest
add_test(NAME FilePathTest COMMAND test_filepath)
