// ExplosionAnimation.h
// Data and logic for the multi-stage explosion animation.
// EDUCATIONAL PURPOSE ONLY.

#pragma once
#include <QColor>
#include <QPointF>

struct EventPoint {
    QString   name;
    double    lat    = 0.0;
    double    lng    = 0.0;
    double    radius = 5.0;   // km
    QString   eventType;
    QColor    color;
};

class ExplosionAnimation
{
public:
    explicit ExplosionAnimation(const EventPoint &point);

    // Advance one frame (~16 ms / 60 FPS).
    void update();

    // Draw explosion onto painter at the given widget size.
    void draw(class QPainter &painter, int widgetW, int widgetH) const;

    bool isComplete() const { return m_progress >= 1.0f; }

private:
    EventPoint m_point;
    float      m_progress = 0.0f;
    // kSpeed = 0.015 → completes in 1/0.015 ≈ 67 frames × 16 ms ≈ 1.07 s
    static constexpr float kSpeed = 0.015f;
};
