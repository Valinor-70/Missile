// DisclaimerDialog.cpp
// EDUCATIONAL PURPOSE ONLY.

#include "DisclaimerDialog.h"
#include <QVBoxLayout>
#include <QHBoxLayout>
#include <QLabel>
#include <QTextEdit>
#include <QPushButton>
#include <QDialogButtonBox>
#include <QFont>

DisclaimerDialog::DisclaimerDialog(QWidget *parent)
    : QDialog(parent)
{
    setWindowTitle("Educational Disclaimer");
    setFixedSize(600, 400);

    auto *layout = new QVBoxLayout(this);
    layout->setContentsMargins(20, 20, 20, 20);
    layout->setSpacing(12);

    auto *title = new QLabel("⚠  Educational Disclaimer", this);
    QFont tf = title->font();
    tf.setPointSize(14);
    tf.setBold(true);
    title->setFont(tf);
    title->setStyleSheet("color: #0078D7;");
    layout->addWidget(title);

    auto *text = new QTextEdit(this);
    text->setReadOnly(true);
    text->setStyleSheet("background:#fff; border:1px solid #ccc;");
    text->setPlainText(
        "EDUCATIONAL DISCLAIMER\n\n"
        "This is a safe simulation for learning mapping, cryptography, and "
        "user-interface design.\n\n"
        "This application demonstrates:\n"
        "  • Geodesic mathematics and coordinate plotting\n"
        "  • AES-256-CBC encryption with HMAC-SHA256 authentication\n"
        "  • PBKDF2 password hashing\n"
        "  • Qt5 animation and painting techniques\n"
        "  • Secure input handling\n\n"
        "NOT INTENDED FOR:\n"
        "This application is NOT intended for planning or enabling any harmful "
        "activity. All simulated events are purely educational examples.\n\n"
        "By clicking 'Accept' you acknowledge this is an educational tool only."
    );
    layout->addWidget(text);

    auto *buttons = new QDialogButtonBox(this);
    auto *accept  = buttons->addButton("Accept",  QDialogButtonBox::AcceptRole);
    auto *decline = buttons->addButton("Decline", QDialogButtonBox::RejectRole);
    accept ->setStyleSheet("background:#0078D7; color:white; padding:6px 18px;");
    decline->setStyleSheet("background:#888;    color:white; padding:6px 18px;");
    connect(buttons, &QDialogButtonBox::accepted, this, &QDialog::accept);
    connect(buttons, &QDialogButtonBox::rejected, this, &QDialog::reject);
    layout->addWidget(buttons);
}
