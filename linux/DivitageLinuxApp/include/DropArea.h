#pragma once

#include <QFrame>

class DropArea final : public QFrame {
    Q_OBJECT
public:
    explicit DropArea(QWidget *parent = nullptr);

signals:
    void filesDropped(const QStringList &paths);

protected:
    void dragEnterEvent(QDragEnterEvent *event) override;
    void dragLeaveEvent(QDragLeaveEvent *event) override;
    void dropEvent(QDropEvent *event) override;

private:
    void setHighlight(bool enabled);
};
