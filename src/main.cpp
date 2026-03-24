// main.cpp
// Entry point for the Qt C++ educational missile simulator.
// EDUCATIONAL PURPOSE ONLY — not intended for real-world use.

#include <QApplication>
#include <QScreen>
#include <QSurfaceFormat>

#include "DisclaimerDialog.h"
#include "AuthDialog.h"
#include "MainWindow.h"

int main(int argc, char *argv[])
{
    // Enable high-DPI scaling for crisp, non-pixelated rendering
    QApplication::setAttribute(Qt::AA_EnableHighDpiScaling);
    QApplication::setAttribute(Qt::AA_UseHighDpiPixmaps);

    QApplication app(argc, argv);

    // Use the Fusion style for a clean, consistent appearance on Linux
    app.setStyle("Fusion");

    // Global font — crisp Sans Serif on Linux
    QFont defaultFont("Noto Sans", 10);
    defaultFont.setHintingPreference(QFont::PreferFullHinting);
    app.setFont(defaultFont);

    // Show disclaimer
    DisclaimerDialog disclaimer;
    if (disclaimer.exec() != QDialog::Accepted)
        return 0;

    // Show authentication dialog
    AuthDialog auth;
    if (auth.exec() != QDialog::Accepted)
        return 0;

    // Launch main window
    MainWindow window(auth.operatorId());
    window.show();

    return app.exec();
}
