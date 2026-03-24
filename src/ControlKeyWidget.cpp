// ControlKeyWidget.cpp
// EDUCATIONAL PURPOSE ONLY.

#include "ControlKeyWidget.h"
#include <QPainter>
#include <QPainterPath>
#include <QMouseEvent>
#include <QtMath>

ControlKeyWidget::ControlKeyWidget(QWidget *parent)
    : QWidget(parent)
{
    setCursor(Qt::OpenHandCursor);
    setFixedSize(200, 200);
}

void ControlKeyWidget::mousePressEvent(QMouseEvent *event)
{
    if (event->button() == Qt::LeftButton) {
        m_dragging = true;
        m_lastPos  = event->pos();
        setCursor(Qt::ClosedHandCursor);
    }
    QWidget::mousePressEvent(event);
}

void ControlKeyWidget::mouseMoveEvent(QMouseEvent *event)
{
    if (m_dragging) {
        int dx  = event->pos().x() - m_lastPos.x();
    static constexpr float kRotationSensitivity = 0.5f; // degrees per pixel of drag
        m_angle += dx * kRotationSensitivity;
        m_angle  = qBound(0.0f, m_angle, 90.0f);

        if (m_angle >= 90.0f && !m_emitted) {
            m_emitted = true;
            emit keyTurned();
        }

        m_lastPos = event->pos();
        update();
    }
    QWidget::mouseMoveEvent(event);
}

void ControlKeyWidget::mouseReleaseEvent(QMouseEvent *event)
{
    m_dragging = false;
    setCursor(Qt::OpenHandCursor);
    QWidget::mouseReleaseEvent(event);
}

void ControlKeyWidget::paintEvent(QPaintEvent *)
{
    QPainter p(this);
    p.setRenderHint(QPainter::Antialiasing);

    int cx = width()  / 2;
    int cy = height() / 2;

    // Housing ring
    p.setBrush(QColor(50, 50, 50));
    p.setPen(QPen(QColor(80, 80, 80), 2));
    p.drawEllipse(QPoint(cx, cy), cx - 20, cy - 20);

    // Rotate key graphic
    p.translate(cx, cy);
    p.rotate(static_cast<double>(m_angle));

    QColor keyColour = isUnlocked() ? QColor(0, 200, 80) : QColor(255, 215, 0);
    p.setBrush(keyColour);
    p.setPen(Qt::NoPen);

    // Key shaft
    p.drawRoundedRect(-8, -55, 16, 70, 4, 4);

    // Key bow (circular ring at top)
    p.drawEllipse(QPoint(0, -55), 18, 18);
    p.setBrush(QColor(50, 50, 50));
    p.drawEllipse(QPoint(0, -55), 10, 10);

    // Key teeth
    p.setBrush(keyColour);
    p.drawRect(8, 0, 8, 6);
    p.drawRect(8, 15, 8, 6);

    p.resetTransform();

    // Lock / Unlock label
    QString label = isUnlocked() ? "UNLOCKED" : "LOCKED";
    QColor  lc    = isUnlocked() ? QColor(0, 200, 80) : QColor(220, 50, 50);
    p.setPen(lc);
    p.setFont(QFont("monospace", 9, QFont::Bold));
    QRect textRect(0, height() - 22, width(), 20);
    p.drawText(textRect, Qt::AlignCenter, label);
}
