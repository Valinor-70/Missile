cmake_minimum_required(VERSION 3.16)
project(MissileSimulator VERSION 1.0 LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Qt5 auto-moc/uic
set(CMAKE_AUTOMOC ON)
set(CMAKE_AUTOUIC ON)
set(CMAKE_AUTORCC ON)

find_package(Qt5 REQUIRED COMPONENTS Widgets)
find_package(OpenSSL REQUIRED)

set(SOURCES
    src/main.cpp
    src/CryptoHelper.cpp
    src/DisclaimerDialog.cpp
    src/AuthDialog.cpp
    src/MapWidget.cpp
    src/ControlKeyWidget.cpp
    src/ExplosionAnimation.cpp
    src/MainWindow.cpp
)

set(HEADERS
    src/CryptoHelper.h
    src/DisclaimerDialog.h
    src/AuthDialog.h
    src/MapWidget.h
    src/ControlKeyWidget.h
    src/ExplosionAnimation.h
    src/MainWindow.h
)

add_executable(${PROJECT_NAME} ${SOURCES} ${HEADERS})

target_link_libraries(${PROJECT_NAME}
    Qt5::Widgets
    OpenSSL::SSL
    OpenSSL::Crypto
)

# Enable high-DPI support via compile definition
target_compile_definitions(${PROJECT_NAME} PRIVATE QT_NO_DEPRECATED_WARNINGS)
