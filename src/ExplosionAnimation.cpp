// ExplosionAnimation.cpp
// EDUCATIONAL PURPOSE ONLY.

#include "ExplosionAnimation.h"
#include <QPainter>
#include <cmath>

ExplosionAnimation::ExplosionAnimation(const EventPoint &point)
    : m_point(point)
{}

void ExplosionAnimation::update()
{
    m_progress += kSpeed;
    if (m_progress > 1.0f) m_progress = 1.0f;
}

void ExplosionAnimation::draw(QPainter &painter, int widgetW, int widgetH) const
{
    painter.save();
    painter.setRenderHint(QPainter::Antialiasing);

    // Simple equirectangular projection centred on widget
    float x = widgetW / 2.0f + static_cast<float>(m_point.lng * 3.0);
    float y = widgetH / 2.0f - static_cast<float>(m_point.lat * 3.0);

    // Expanding radius
    float maxRadius = static_cast<float>(m_point.radius * 5 * 3);
    float radius    = maxRadius * m_progress;
    int   alpha     = static_cast<int>(255 * (1.0f - m_progress));
    alpha = qMax(0, alpha);

    auto fillEllipse = [&](float r, int a, QColor colour) {
        colour.setAlpha(qMax(0, a));
        painter.setBrush(colour);
        painter.setPen(Qt::NoPen);
        painter.drawEllipse(QPointF(x, y), r, r);
    };

    // Three-layer explosion: blast wave → fireball → white-hot core
    fillEllipse(radius,        alpha / 2, QColor(255, 100, 0));   // outer blast
    fillEllipse(radius * 0.6f, alpha,     QColor(255, 200, 0));   // fireball
    fillEllipse(radius * 0.3f, alpha,     QColor(255, 255, 255)); // core

    painter.restore();
}
