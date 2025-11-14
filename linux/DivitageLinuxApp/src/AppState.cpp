#include "AppState.h"

#include <QDateTime>
#include <QDir>
#include <QFileInfo>
#include <QStandardPaths>
#include <QVariant>

AppState::AppState(QObject *parent)
    : QObject(parent)
    , m_settings("Divitage", "Divitage")
{
    qRegisterMetaType<ConversionTask>("ConversionTask");
    QString defaultDir = QStandardPaths::writableLocation(QStandardPaths::DownloadLocation);
    if (defaultDir.isEmpty()) {
        defaultDir = QDir::homePath();
    }
    defaultDir += "/DivitageOutput";

    m_outputDirectory = m_settings.value("outputDirectory", defaultDir).toString();
    if (m_outputDirectory.isEmpty()) {
        m_outputDirectory = defaultDir;
    }
    QDir().mkpath(m_outputDirectory);
}

AppState::~AppState() {
    if (m_worker) {
        m_worker->requestInterruption();
        m_worker->wait(200);
    }
}

QString AppState::outputDirectory() const {
    return m_outputDirectory;
}

QStringList AppState::logs() const {
    return m_logs;
}

int AppState::enqueueSources(const QStringList &paths) {
    int added = 0;
    for (const QString &path : paths) {
        QFileInfo info(path);
        if (!info.exists()) {
            appendLog(tr("%1 が見つかりませんでした").arg(path));
            continue;
        }
        const QString source = info.absoluteFilePath();
        const QString dest = nextDestinationFor(source);
        ConversionTask task{source, dest, info.fileName().isEmpty() ? source : info.fileName()};
        m_queue.enqueue(task);
        ++added;
    }
    if (added) {
        appendLog(tr("%1 件のタスクを追加しました").arg(added));
        emit queueLengthChanged(m_queue.size());
    }
    return added;
}

bool AppState::startProcessing() {
    if (m_isProcessing || m_queue.isEmpty()) {
        return false;
    }
    processNext();
    return true;
}

void AppState::setOutputDirectory(const QString &directory) {
    if (directory.isEmpty()) {
        return;
    }
    QDir().mkpath(directory);
    m_outputDirectory = directory;
    m_settings.setValue("outputDirectory", directory);
    emit outputDirectoryChanged(directory);
}

bool AppState::settingValue(const QString &key, bool defaultValue) const {
    return m_settings.value(key, defaultValue).toBool();
}

void AppState::setSettingValue(const QString &key, bool value) {
    m_settings.setValue(key, value);
}

void AppState::handleWorkerFinished(bool success, const QString &errorMessage) {
    if (!m_worker) {
        return;
    }
    ConversionWorker *worker = m_worker;
    m_worker = nullptr;
    worker->deleteLater();

    const ConversionTask task = worker->property("task").value<ConversionTask>();
    if (success) {
        appendLog(tr("%1 -> %2 変換完了").arg(task.displayName, QFileInfo(task.destinationPath).fileName()));
    } else {
        appendLog(tr("%1 の変換に失敗しました: %2").arg(task.displayName, errorMessage));
    }

    if (!m_queue.isEmpty()) {
        processNext();
    } else {
        m_isProcessing = false;
        emit processingStateChanged(false);
        emit conversionCycleFinished(tr("キューの処理が完了しました"));
    }
}

void AppState::appendLog(const QString &message) {
    const QString timestamp = QDateTime::currentDateTime().toString("HH:mm:ss");
    const QString line = QStringLiteral("[%1] %2").arg(timestamp, message);
    m_logs.append(line);
    emit logAppended(line);
}

void AppState::processNext() {
    if (m_queue.isEmpty()) {
        return;
    }
    const ConversionTask task = m_queue.dequeue();
    emit queueLengthChanged(m_queue.size());

    m_worker = new ConversionWorker(task, this);
    m_worker->setProperty("task", QVariant::fromValue(task));
    connect(m_worker, &ConversionWorker::logMessage, this, &AppState::appendLog);
    connect(m_worker, &ConversionWorker::finished, this, &AppState::handleWorkerFinished);
    m_worker->start();
    m_isProcessing = true;
    emit processingStateChanged(true);
}

QString AppState::nextDestinationFor(const QString &sourcePath) const {
    QFileInfo sourceInfo(sourcePath);
    QString fileName = sourceInfo.fileName();
    if (fileName.isEmpty()) {
        fileName = sourcePath;
    }

    QString stem;
    QString suffix;
    if (sourceInfo.isFile()) {
        stem = sourceInfo.completeBaseName();
        suffix = sourceInfo.completeSuffix().isEmpty() ? QString() : QStringLiteral(".") + sourceInfo.completeSuffix();
    } else {
        stem = fileName;
    }

    QString candidate = QDir(m_outputDirectory).filePath(fileName);
    QFileInfo destInfo(candidate);
    int counter = 1;
    while (destInfo.exists()) {
        const QString suffixed = suffix.isEmpty()
                                     ? QStringLiteral("%1_%2").arg(stem, QString::number(counter))
                                     : QStringLiteral("%1_%2%3").arg(stem, QString::number(counter), suffix);
        candidate = QDir(m_outputDirectory).filePath(suffixed);
        destInfo.setFile(candidate);
        ++counter;
    }
    return candidate;
}
