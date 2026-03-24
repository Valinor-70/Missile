// AuthDialog.h
// EDUCATIONAL PURPOSE ONLY.

#pragma once
#include <QDialog>
#include <QString>

class QLineEdit;
class QLabel;

class AuthDialog : public QDialog
{
    Q_OBJECT
public:
    explicit AuthDialog(QWidget *parent = nullptr);

    QString operatorId() const;

private slots:
    void onLogin();

private:
    QLineEdit *m_operatorEdit = nullptr;
    QLineEdit *m_passphraseEdit = nullptr;
    QLabel    *m_statusLabel    = nullptr;

    // Pre-hashed demo credentials (generated once on first use)
    static QString s_hash1, s_salt1; // O5-X
    static QString s_hash2, s_salt2; // Ethics Committee

    static void ensureCredentials();
};
