// MapWidget.h
// Custom QWidget that paints the map grid, event points and explosion animation.
// EDUCATIONAL PURPOSE ONLY.

#pragma once
#include <QWidget>
#include <QTimer>
#include <vector>
#include <memory>
#include "ExplosionAnimation.h"
#include "ControlKeyWidget.h"

class MapWidget : public QWidget
{
    Q_OBJECT
public:
    explicit MapWidget(QWidget *parent = nullptr);

    void addEventPoint(const EventPoint &pt);
    void triggerExplosion(const EventPoint &pt);

    ControlKeyWidget *controlKey() { return m_controlKey; }

protected:
    void paintEvent(QPaintEvent *event) override;

private slots:
    void onAnimationTick();

private:
    void drawEventPoint(QPainter &p, const EventPoint &pt) const;

    std::vector<EventPoint>              m_points;
    std::unique_ptr<ExplosionAnimation>  m_explosion;
    QTimer                               m_timer;
    ControlKeyWidget                    *m_controlKey = nullptr;
};
