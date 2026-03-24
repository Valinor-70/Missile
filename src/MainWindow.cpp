// MainWindow.cpp
// EDUCATIONAL PURPOSE ONLY.

#include "MainWindow.h"
#include "MapWidget.h"
#include "CryptoHelper.h"

#include <QHBoxLayout>
#include <QVBoxLayout>
#include <QLabel>
#include <QComboBox>
#include <QLineEdit>
#include <QPushButton>
#include <QTextEdit>
#include <QScrollArea>
#include <QWidget>
#include <QFrame>
#include <QMessageBox>
#include <QInputDialog>
#include <QDateTime>
#include <QFont>
#include <QColor>
#include <QSizePolicy>
#include <stdexcept>
#include <cmath>

// ─── small style helpers ─────────────────────────────────────────────────────

static QPushButton *mkBtn(QWidget *parent, const QString &text,
                           const QString &bg = "#0078D7")
{
    auto *b = new QPushButton(text, parent);
    b->setStyleSheet(
        QString("QPushButton{"
                "  background:%1; color:white; font-weight:bold;"
                "  padding:8px 4px; border:none; border-radius:4px;}"
                "QPushButton:disabled{background:#888;}"
                "QPushButton:hover{background:%1; opacity:.9;}").arg(bg));
    b->setMinimumHeight(38);
    b->setSizePolicy(QSizePolicy::Expanding, QSizePolicy::Fixed);
    return b;
}

static QLabel *mkLabel(QWidget *parent, const QString &text, int pt = 10,
                        bool bold = false, const QString &colour = {})
{
    auto *l = new QLabel(text, parent);
    QFont f; f.setPointSize(pt); f.setBold(bold);
    l->setFont(f);
    if (!colour.isEmpty())
        l->setStyleSheet("color:" + colour + ";");
    return l;
}

static QLineEdit *mkEdit(QWidget *parent, const QString &def = {})
{
    auto *e = new QLineEdit(parent);
    e->setText(def);
    e->setFixedHeight(30);
    e->setStyleSheet("padding:4px; border:1px solid #ccc; border-radius:3px;");
    return e;
}

// ─── Constructor ─────────────────────────────────────────────────────────────

MainWindow::MainWindow(const QString &operatorId, QWidget *parent)
    : QMainWindow(parent)
    , m_operatorId(operatorId)
{
    setWindowTitle("Educational Missile Simulator — SAFE DEMONSTRATION");
    setMinimumSize(1400, 900);
    setupUi();
    logEvent(QString("Operator %1 logged in").arg(m_operatorId));
}

// ─── UI setup ────────────────────────────────────────────────────────────────

void MainWindow::setupUi()
{
    auto *central  = new QWidget(this);
    auto *topLayout = new QHBoxLayout(central);
    topLayout->setContentsMargins(0, 0, 0, 0);
    topLayout->setSpacing(0);
    setCentralWidget(central);

    // ── Map area ──────────────────────────────────────────────────────────
    m_mapWidget = new MapWidget(central);
    topLayout->addWidget(m_mapWidget, 1);

    connect(m_mapWidget->controlKey(), &ControlKeyWidget::keyTurned,
            this, &MainWindow::onControlKeyTurned);

    // ── Right panel in a scroll area ─────────────────────────────────────
    auto *scroll = new QScrollArea(central);
    scroll->setWidgetResizable(true);
    scroll->setFixedWidth(500);
    scroll->setHorizontalScrollBarPolicy(Qt::ScrollBarAlwaysOff);

    auto *panel = new QWidget;
    panel->setStyleSheet("QWidget{background:white;}");
    scroll->setWidget(panel);
    topLayout->addWidget(scroll, 0);

    auto *vl = new QVBoxLayout(panel);
    vl->setContentsMargins(20, 20, 20, 20);
    vl->setSpacing(6);

    auto addSection = [&](const QString &title) {
        auto *sep = new QFrame(panel); sep->setFrameShape(QFrame::HLine);
        sep->setStyleSheet("color:#ddd;");
        vl->addSpacing(8);
        vl->addWidget(sep);
        vl->addWidget(mkLabel(panel, title, 12, true, "#0078D7"));
        vl->addSpacing(2);
    };

    // Title
    vl->addWidget(mkLabel(panel, "Control Panel", 14, true, "#0078D7"));
    vl->addSpacing(6);

    // ── Missile type ──────────────────────────────────────────────────────
    vl->addWidget(mkLabel(panel, "Missile Type:"));
    m_typeCombo = new QComboBox(panel);
    m_typeCombo->addItems({"Conventional", "Nuclear", "Thermonuclear", "Thaumonuclear"});
    m_typeCombo->setFixedHeight(30);
    vl->addWidget(m_typeCombo);
    connect(m_typeCombo, QOverload<int>::of(&QComboBox::currentIndexChanged),
            this, &MainWindow::onMissileTypeChanged);

    // ── Coordinates ───────────────────────────────────────────────────────
    vl->addWidget(mkLabel(panel, "Latitude:"));
    m_latEdit = mkEdit(panel, "40.7128");
    vl->addWidget(m_latEdit);

    vl->addWidget(mkLabel(panel, "Longitude:"));
    m_lngEdit = mkEdit(panel, "-74.0060");
    vl->addWidget(m_lngEdit);

    vl->addWidget(mkLabel(panel, "Radius (km):"));
    m_radiusEdit = mkEdit(panel, "5");
    vl->addWidget(m_radiusEdit);

    {
        auto *plotBtn = mkBtn(panel, "Plot Event Point");
        vl->addWidget(plotBtn);
        connect(plotBtn, &QPushButton::clicked, this, &MainWindow::onPlot);
    }

    // ── Missile ID & required code ────────────────────────────────────────
    addSection("Mission Identifiers");

    vl->addWidget(mkLabel(panel, "Missile ID:"));
    m_missileIdLabel = mkLabel(panel, "None", 10, true, "#00008B");
    m_missileIdLabel->setFont(QFont("Monospace", 10, QFont::Bold));
    vl->addWidget(m_missileIdLabel);

    vl->addWidget(mkLabel(panel, "Required Launch Code:"));
    m_reqCodeLabel = mkLabel(panel, "XXX-XX-XXX", 12, true, "#DC3545");
    m_reqCodeLabel->setFont(QFont("Monospace", 12, QFont::Bold));
    vl->addWidget(m_reqCodeLabel);

    // ── Launch code input (auto-dash + correct cursor) ────────────────────
    vl->addWidget(mkLabel(panel, "Enter Launch Code (XXX-XX-XXX):"));
    m_launchCodeEdit = new QLineEdit(panel);
    m_launchCodeEdit->setMaxLength(10);
    m_launchCodeEdit->setFixedHeight(36);
    m_launchCodeEdit->setFont(QFont("Monospace", 14, QFont::Bold));
    m_launchCodeEdit->setStyleSheet(
        "padding:4px; border:2px solid #0078D7; border-radius:4px; letter-spacing:2px;");
    // Use textEdited (not textChanged) to avoid triggering when we set text programmatically
    connect(m_launchCodeEdit, &QLineEdit::textEdited,
            this, &MainWindow::onLaunchCodeTextEdited);
    vl->addWidget(m_launchCodeEdit);

    // ── Encryption demo ───────────────────────────────────────────────────
    addSection("Encryption Demo (AES-256-CBC)");

    vl->addWidget(mkLabel(panel, "Encrypted Payload:"));
    m_encryptedEdit = new QTextEdit(panel);
    m_encryptedEdit->setReadOnly(true);
    m_encryptedEdit->setFixedHeight(90);
    m_encryptedEdit->setFont(QFont("Monospace", 8));
    m_encryptedEdit->setStyleSheet("background:#f8f8f8; border:1px solid #ccc;");
    vl->addWidget(m_encryptedEdit);

    {
        auto *encBtn = mkBtn(panel, "Encrypt Activation Data");
        vl->addWidget(encBtn);
        connect(encBtn, &QPushButton::clicked, this, &MainWindow::onEncrypt);
    }
    {
        auto *decBtn = mkBtn(panel, "Decrypt & Verify");
        vl->addWidget(decBtn);
        connect(decBtn, &QPushButton::clicked, this, &MainWindow::onDecrypt);
    }

    m_activateBtn = mkBtn(panel, "🚀  ACTIVATE SIMULATION", "#DC3545");
    m_activateBtn->setEnabled(false);
    vl->addWidget(m_activateBtn);
    connect(m_activateBtn, &QPushButton::clicked, this, &MainWindow::onActivate);

    // ── Event log (green-on-black terminal style) ─────────────────────────
    addSection("Event Log");

    m_consoleEdit = new QTextEdit(panel);
    m_consoleEdit->setReadOnly(true);
    m_consoleEdit->setFixedHeight(160);
    m_consoleEdit->setFont(QFont("Monospace", 9));
    m_consoleEdit->setStyleSheet(
        "background:#0D0D0D; color:#00FF41;"
        "border:1px solid #333; border-radius:4px;");
    vl->addWidget(m_consoleEdit);

    vl->addStretch();
}

// ─── Slots ───────────────────────────────────────────────────────────────────

void MainWindow::onMissileTypeChanged(int /*index*/)
{
    logEvent("Missile type changed to: " + m_typeCombo->currentText());
}

void MainWindow::onControlKeyTurned()
{
    logEvent("Control key turned — system unlocked");
}

void MainWindow::onPlot()
{
    bool ok1, ok2, ok3;
    double lat    = m_latEdit->text().toDouble(&ok1);
    double lng    = m_lngEdit->text().toDouble(&ok2);
    double radius = m_radiusEdit->text().toDouble(&ok3);

    if (!ok1 || !ok2 || !ok3) {
        QMessageBox::critical(this, "Error", "Invalid coordinates or radius.");
        return;
    }

    QString type = m_typeCombo->currentText();
    m_currentMissileId = QString("%1-%2").arg(type.toUpper())
                             .arg(std::abs(lat * lng), 0, 'f', 2);
    m_missileIdLabel->setText(m_currentMissileId);

    m_requiredLaunchCode =
        QString::fromStdString(CryptoHelper::generateLaunchCode());
    m_reqCodeLabel->setText(m_requiredLaunchCode);

    EventPoint pt;
    pt.name      = type;
    pt.lat       = lat;
    pt.lng       = lng;
    pt.radius    = radius;
    pt.eventType = type;
    pt.color     = colorForType(type);

    m_eventPoints.append(pt);
    m_mapWidget->addEventPoint(pt);

    logEvent(QString("Event plotted: %1 at (%2, %3), r=%4 km")
                 .arg(type).arg(lat).arg(lng).arg(radius));
    logEvent("Missile ID: " + m_currentMissileId);
    logEvent("Launch code required: " + m_requiredLaunchCode);
}

void MainWindow::onEncrypt()
{
    if (m_currentMissileId.isEmpty()) {
        QMessageBox::information(this, "Info", "Please plot an event point first.");
        return;
    }

    QString payload = QString(
        R"({"operator":"%1","event":"%2","coords":[%3,%4],"radius_km":%5})")
        .arg(m_operatorId)
        .arg(m_typeCombo->currentText())
        .arg(m_latEdit->text())
        .arg(m_lngEdit->text())
        .arg(m_radiusEdit->text());

    m_encryptionKey = QString::fromStdString(CryptoHelper::generateLaunchCode());

    try {
        std::string enc = CryptoHelper::encryptAES(payload.toStdString(),
                                                    m_encryptionKey.toStdString());
        m_encryptedEdit->setPlainText(QString::fromStdString(enc));
        logEvent("Payload encrypted. Key: " + m_encryptionKey);
        QMessageBox::information(this, "Encryption Complete",
            "Encryption successful!\n\nEncryption Key: " + m_encryptionKey +
            "\n\nStore this key for decryption.");
    } catch (const std::exception &ex) {
        QMessageBox::critical(this, "Error",
            QString("Encryption failed: ") + ex.what());
    }
}

void MainWindow::onDecrypt()
{
    if (m_encryptedEdit->toPlainText().isEmpty()) {
        QMessageBox::information(this, "Info", "No encrypted data available.");
        return;
    }

    bool ok;
    QString key = QInputDialog::getText(this, "Decrypt",
                                        "Enter decryption key:",
                                        QLineEdit::Normal,
                                        m_encryptionKey, &ok);
    if (!ok || key.isEmpty()) return;

    try {
        std::string dec = CryptoHelper::decryptAES(
            m_encryptedEdit->toPlainText().toStdString(),
            key.toStdString());
        logEvent("Decryption successful");
        logEvent("Payload: " + QString::fromStdString(dec));
        QMessageBox::information(this, "Decryption Successful",
            "Decrypted payload:\n\n" + QString::fromStdString(dec));
        m_activateBtn->setEnabled(true);
    } catch (const std::exception &ex) {
        logEvent(QString("Decryption failed: ") + ex.what());
        QMessageBox::critical(this, "Error",
            QString("Decryption failed: ") + ex.what());
    }
}

void MainWindow::onActivate()
{
    QString entered = m_launchCodeEdit->text().trimmed();
    if (entered != m_requiredLaunchCode) {
        QMessageBox::critical(this, "Authentication Failed",
            "Invalid launch code!\n\nRequired: " + m_requiredLaunchCode +
            "\nEntered:  " + entered);
        logEvent("Activation failed: invalid launch code");
        return;
    }

    if (!m_mapWidget->controlKey()->isUnlocked()) {
        QMessageBox::warning(this, "Error", "Control key must be turned first!");
        return;
    }

    if (!m_eventPoints.isEmpty()) {
        const EventPoint &last = m_eventPoints.last();
        m_mapWidget->triggerExplosion(last);
        logEvent(QString("SIMULATION ACTIVATED — %1 event at (%2, %3)")
                     .arg(last.eventType).arg(last.lat).arg(last.lng));
        QMessageBox::information(this, "Activation Complete",
            "Simulation activated!\n\nWatch the map for the explosion animation.");
    }
}

// ─── Launch code: auto-dash + correct cursor (KEY FIX) ───────────────────────
//
// Problem: when the user types a digit and we re-insert dashes, Qt would reset
// the cursor to position 0 or leave it BEHIND the inserted dash, causing the
// "cursor behind number" bug.
//
// Fix: after reformatting, we call setCursorPosition() explicitly to put the
// cursor at the END of the formatted string.

void MainWindow::onLaunchCodeTextEdited(const QString &text)
{
    if (m_updatingLaunchCode) return;
    m_updatingLaunchCode = true;

    // 1. Strip everything that is not a digit
    QString digits;
    for (QChar c : text) {
        if (c.isDigit()) digits.append(c);
        if (digits.size() == 8) break; // max 8 digits: XXX-XX-XXX
    }

    // 2. Build formatted string XXX-XX-XXX
    QString formatted;
    for (int i = 0; i < digits.size(); ++i) {
        if (i == 3 || i == 5) formatted += '-';
        formatted += digits[i];
    }

    // 3. Update the widget and place cursor AFTER the last character
    //    (fixes the "cursor behind number" issue)
    m_launchCodeEdit->setText(formatted);
    m_launchCodeEdit->setCursorPosition(formatted.size()); // KEY FIX

    m_updatingLaunchCode = false;
}

// ─── Utilities ───────────────────────────────────────────────────────────────

void MainWindow::logEvent(const QString &msg)
{
    QString ts = QDateTime::currentDateTime().toString("HH:mm:ss");
    m_consoleEdit->append("[" + ts + "] " + msg);
}

QColor MainWindow::colorForType(const QString &type) const
{
    if (type == "Conventional") return QColor(255, 165,   0);
    if (type == "Nuclear")      return QColor(255,   0,   0);
    if (type == "Thermonuclear")return QColor(255,   0, 255);
    if (type == "Thaumonuclear")return QColor(  0, 255, 255);
    return Qt::gray;
}
