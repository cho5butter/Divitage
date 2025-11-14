#include "ConversionWorker.h"

#include <QDir>
#include <QFile>
#include <QFileInfo>
#include <QThread>

ConversionWorker::ConversionWorker(const ConversionTask &task, QObject *parent)
    : QThread(parent)
    , m_task(task) {
    qRegisterMetaType<ConversionTask>("ConversionTask");
}

void ConversionWorker::run() {
    emit logMessage(tr("変換開始: %1").arg(m_task.displayName));
    bool success = copyRecursively(m_task.sourcePath, m_task.destinationPath);
    msleep(200); // 擬似的な処理ディレイで UI 表示にゆとりを持たせる
    if (success) {
        emit finished(true, {});
    } else {
        emit finished(false, tr("コピーに失敗しました"));
    }
}

bool ConversionWorker::copyRecursively(const QString &source, const QString &destination) {
    QFileInfo srcInfo(source);
    if (!srcInfo.exists()) {
        return false;
    }

    if (srcInfo.isDir()) {
        QDir srcDir(source);
        QDir destDir;
        if (!destDir.mkpath(destination)) {
            return false;
        }
        const auto entries = srcDir.entryInfoList(QDir::NoDotAndDotDot | QDir::AllEntries);
        for (const QFileInfo &entry : entries) {
            const QString childSrc = entry.absoluteFilePath();
            const QString childDest = destination + QLatin1Char('/') + entry.fileName();
            if (!copyRecursively(childSrc, childDest)) {
                return false;
            }
        }
        return true;
    }

    QDir destDir;
    destDir.mkpath(QFileInfo(destination).absolutePath());
    QFile::remove(destination);
    return QFile::copy(source, destination);
}
