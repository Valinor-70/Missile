// MapWidget.cpp
// EDUCATIONAL PURPOSE ONLY.

#include "MapWidget.h"
#include <QPainter>
#include <QPen>

MapWidget::MapWidget(QWidget *parent)
    : QWidget(parent)
{
    setMinimumSize(900, 900);
    setAutoFillBackground(false);

    // 60 FPS animation timer
    connect(&m_timer, &QTimer::timeout, this, &MapWidget::onAnimationTick);
    m_timer.start(16);

    // Embed the control key in the centre of the map
    m_controlKey = new ControlKeyWidget(this);
    m_controlKey->move((900 - 200) / 2, (900 - 200) / 2);
}

void MapWidget::addEventPoint(const EventPoint &pt)
{
    m_points.push_back(pt);
    update();
}

void MapWidget::triggerExplosion(const EventPoint &pt)
{
    m_explosion = std::make_unique<ExplosionAnimation>(pt);
}

void MapWidget::onAnimationTick()
{
    if (m_explosion && !m_explosion->isComplete()) {
        m_explosion->update();
        update();
    }
}

void MapWidget::paintEvent(QPaintEvent *)
{
    QPainter p(this);
    p.setRenderHint(QPainter::Antialiasing);
    p.setRenderHint(QPainter::TextAntialiasing);
    p.setRenderHint(QPainter::SmoothPixmapTransform);

    // Background
    p.fillRect(rect(), QColor(230, 240, 250));

    // Grid
    p.setPen(QPen(QColor(200, 220, 230), 1));
    for (int x = 0; x < width(); x += 50)
        p.drawLine(x, 0, x, height());
    for (int y = 0; y < height(); y += 50)
        p.drawLine(0, y, width(), y);

    // Event points
    for (const auto &pt : m_points)
        drawEventPoint(p, pt);

    // Explosion animation
    if (m_explosion && !m_explosion->isComplete())
        m_explosion->draw(p, width(), height());
}

void MapWidget::drawEventPoint(QPainter &p, const EventPoint &pt) const
{
    float x = width()  / 2.0f + static_cast<float>(pt.lng * 3.0);
    float y = height() / 2.0f - static_cast<float>(pt.lat * 3.0);

    float outerR = static_cast<float>(pt.radius * 5.0);
    float innerR = outerR * 0.5f;

    // Outer effect zone
    QColor outer = pt.color;
    outer.setAlpha(30);
    p.setBrush(outer);
    p.setPen(Qt::NoPen);
    p.drawEllipse(QPointF(x, y), outerR, outerR);

    // Inner impact zone
    outer.setAlpha(60);
    p.setBrush(outer);
    p.drawEllipse(QPointF(x, y), innerR, innerR);

    // Centre marker
    p.setBrush(pt.color);
    p.drawEllipse(QPointF(x, y), 5.0, 5.0);

    // Label
    p.setPen(Qt::black);
    p.setFont(QFont("Sans Serif", 9, QFont::Bold));
    p.drawText(static_cast<int>(x) + 8, static_cast<int>(y) - 4, pt.name);
}
