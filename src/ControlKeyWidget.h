// ControlKeyWidget.h
// Interactive drag-to-rotate key widget.
// EDUCATIONAL PURPOSE ONLY.

#pragma once
#include <QWidget>

class ControlKeyWidget : public QWidget
{
    Q_OBJECT
public:
    explicit ControlKeyWidget(QWidget *parent = nullptr);

    bool isUnlocked()  const { return m_angle >= 90.0f; }

signals:
    void keyTurned();

protected:
    void mousePressEvent(QMouseEvent *event) override;
    void mouseMoveEvent(QMouseEvent *event) override;
    void mouseReleaseEvent(QMouseEvent *event) override;
    void paintEvent(QPaintEvent *event) override;

private:
    float  m_angle     = 0.0f;
    bool   m_dragging  = false;
    QPoint m_lastPos;
    bool   m_emitted   = false;
};
