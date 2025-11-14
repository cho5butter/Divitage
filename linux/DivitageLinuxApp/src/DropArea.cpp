#include "DropArea.h"

#include <QDragEnterEvent>
#include <QDropEvent>
#include <QMimeData>
#include <QUrl>
#include <QPalette>

DropArea::DropArea(QWidget *parent)
    : QFrame(parent)
{
    setObjectName("DropArea");
    setAcceptDrops(true);
    setMinimumHeight(160);
    setFrameShape(QFrame::StyledPanel);
    setStyleSheet(R"(
        #DropArea {
            border: 2px dashed palette(Mid);
            border-radius: 12px;
            background-color: rgba(0, 0, 0, 20);
        }
    )");
}

void DropArea::dragEnterEvent(QDragEnterEvent *event) {
    if (event->mimeData()->hasUrls()) {
        event->acceptProposedAction();
        setHighlight(true);
    } else {
        event->ignore();
    }
}

void DropArea::dragLeaveEvent(QDragLeaveEvent *event) {
    QFrame::dragLeaveEvent(event);
    setHighlight(false);
}

void DropArea::dropEvent(QDropEvent *event) {
    setHighlight(false);
    if (!event->mimeData()->hasUrls()) {
        event->ignore();
        return;
    }

    QStringList paths;
    for (const QUrl &url : event->mimeData()->urls()) {
        if (url.isLocalFile()) {
            paths << url.toLocalFile();
        }
    }
    if (!paths.isEmpty()) {
        emit filesDropped(paths);
    }
    event->acceptProposedAction();
}

void DropArea::setHighlight(bool enabled) {
    if (enabled) {
        setStyleSheet(R"(
            #DropArea {
                border: 2px solid palette(Highlight);
                border-radius: 12px;
                background-color: rgba(50, 150, 255, 30);
            }
        )");
    } else {
        setStyleSheet(R"(
            #DropArea {
                border: 2px dashed palette(Mid);
                border-radius: 12px;
                background-color: rgba(0, 0, 0, 20);
            }
        )");
    }
}
