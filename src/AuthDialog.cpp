// AuthDialog.cpp
// EDUCATIONAL PURPOSE ONLY.

#include "AuthDialog.h"
#include "CryptoHelper.h"

#include <QVBoxLayout>
#include <QFormLayout>
#include <QHBoxLayout>
#include <QLabel>
#include <QLineEdit>
#include <QPushButton>
#include <QFont>
#include <QMap>

// Static credential store (populated once)
QString AuthDialog::s_hash1, AuthDialog::s_salt1;
QString AuthDialog::s_hash2, AuthDialog::s_salt2;

void AuthDialog::ensureCredentials()
{
    if (!s_hash1.isEmpty()) return;

    std::string salt1, salt2;
    s_hash1 = QString::fromStdString(CryptoHelper::hashPassword("HandofDemocracy", salt1));
    s_salt1 = QString::fromStdString(salt1);
    s_hash2 = QString::fromStdString(CryptoHelper::hashPassword("HandofDemocracy", salt2));
    s_salt2 = QString::fromStdString(salt2);
}

AuthDialog::AuthDialog(QWidget *parent)
    : QDialog(parent)
{
    ensureCredentials();

    setWindowTitle("Operator Authentication");
    setFixedSize(480, 320);
    setStyleSheet("background:#F0F0F0;");

    auto *root = new QVBoxLayout(this);
    root->setContentsMargins(40, 30, 40, 30);
    root->setSpacing(14);

    auto *title = new QLabel("🔐  Operator Authentication", this);
    QFont tf; tf.setPointSize(15); tf.setBold(true);
    title->setFont(tf);
    title->setStyleSheet("color:#0078D7;");
    root->addWidget(title);

    auto *form = new QFormLayout();
    form->setSpacing(10);

    m_operatorEdit = new QLineEdit(this);
    m_operatorEdit->setPlaceholderText("e.g. O5-X");
    m_operatorEdit->setFixedHeight(32);
    form->addRow("Operator ID:", m_operatorEdit);

    m_passphraseEdit = new QLineEdit(this);
    m_passphraseEdit->setEchoMode(QLineEdit::Password);
    m_passphraseEdit->setFixedHeight(32);
    form->addRow("Passphrase:", m_passphraseEdit);
    root->addLayout(form);

    m_statusLabel = new QLabel(this);
    m_statusLabel->setStyleSheet("color:red;");
    root->addWidget(m_statusLabel);

    auto *btnRow = new QHBoxLayout();
    btnRow->addStretch();
    auto *cancelBtn = new QPushButton("Cancel", this);
    cancelBtn->setStyleSheet("background:#888; color:white; padding:6px 18px;");
    auto *loginBtn  = new QPushButton("Login",  this);
    loginBtn->setStyleSheet("background:#0078D7; color:white; padding:6px 18px; font-weight:bold;");
    btnRow->addWidget(cancelBtn);
    btnRow->addWidget(loginBtn);
    root->addLayout(btnRow);

    connect(loginBtn,  &QPushButton::clicked, this, &AuthDialog::onLogin);
    connect(cancelBtn, &QPushButton::clicked, this, &QDialog::reject);
    connect(m_passphraseEdit, &QLineEdit::returnPressed, this, &AuthDialog::onLogin);
}

QString AuthDialog::operatorId() const
{
    return m_operatorEdit->text().trimmed();
}

void AuthDialog::onLogin()
{
    QString id   = m_operatorEdit->text().trimmed();
    QString pass = m_passphraseEdit->text();

    if (id.isEmpty() || pass.isEmpty()) {
        m_statusLabel->setText("Please enter both Operator ID and Passphrase.");
        return;
    }

    // Map of known operators
    QMap<QString, QPair<QString,QString>> creds;
    creds["O5-X"]            = {s_hash1, s_salt1};
    creds["Ethics Committee"] = {s_hash2, s_salt2};

    if (creds.contains(id)) {
        const auto &[hash, salt] = creds[id];
        bool ok = CryptoHelper::verifyPassword(
            pass.toStdString(),
            hash.toStdString(),
            salt.toStdString()
        );
        if (ok) {
            accept();
            return;
        }
    }

    m_statusLabel->setText("Invalid credentials. Try: O5-X / HandofDemocracy");
}
