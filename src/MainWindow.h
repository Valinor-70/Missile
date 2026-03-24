// MainWindow.h
// Main application window: map + control panel.
// EDUCATIONAL PURPOSE ONLY.

#pragma once
#include <QMainWindow>
#include <QString>
#include <QList>
#include "ExplosionAnimation.h"

class MapWidget;
class QComboBox;
class QLineEdit;
class QLabel;
class QPushButton;
class QTextEdit;

class MainWindow : public QMainWindow
{
    Q_OBJECT
public:
    explicit MainWindow(const QString &operatorId, QWidget *parent = nullptr);

private slots:
    void onPlot();
    void onEncrypt();
    void onDecrypt();
    void onActivate();
    void onMissileTypeChanged(int index);
    void onControlKeyTurned();

    // Launch-code auto-dash with correct cursor positioning (KEY FIX)
    void onLaunchCodeTextEdited(const QString &text);

private:
    void setupUi();
    void logEvent(const QString &msg);
    QColor colorForType(const QString &type) const;

    // Widgets
    MapWidget   *m_mapWidget       = nullptr;
    QComboBox   *m_typeCombo       = nullptr;
    QLineEdit   *m_latEdit         = nullptr;
    QLineEdit   *m_lngEdit         = nullptr;
    QLineEdit   *m_radiusEdit      = nullptr;
    QLabel      *m_missileIdLabel  = nullptr;
    QLabel      *m_reqCodeLabel    = nullptr;
    QLineEdit   *m_launchCodeEdit  = nullptr;
    QTextEdit   *m_encryptedEdit   = nullptr;
    QPushButton *m_activateBtn     = nullptr;
    QTextEdit   *m_consoleEdit     = nullptr;

    // State
    QString           m_operatorId;
    QString           m_currentMissileId;
    QString           m_requiredLaunchCode;
    QString           m_encryptionKey;
    QList<EventPoint> m_eventPoints;

    bool m_updatingLaunchCode = false;  // re-entrancy guard for auto-dash
};
