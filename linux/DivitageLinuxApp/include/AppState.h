#pragma once

#include <QObject>
#include <QQueue>
#include <QSettings>
#include <QStringList>

#include "ConversionWorker.h"

class AppState final : public QObject {
    Q_OBJECT
public:
    explicit AppState(QObject *parent = nullptr);
    ~AppState() override;

    QString outputDirectory() const;
    QStringList logs() const;

    int enqueueSources(const QStringList &paths);
    bool startProcessing();
    void setOutputDirectory(const QString &directory);

    bool settingValue(const QString &key, bool defaultValue) const;
    void setSettingValue(const QString &key, bool value);

signals:
    void logAppended(const QString &logLine);
    void queueLengthChanged(int queueLength);
    void processingStateChanged(bool isProcessing);
    void outputDirectoryChanged(const QString &directory);
    void conversionCycleFinished(const QString &message);

private slots:
    void handleWorkerFinished(bool success, const QString &errorMessage);

private:
    void appendLog(const QString &message);
    void processNext();
    QString nextDestinationFor(const QString &sourcePath) const;

    QQueue<ConversionTask> m_queue;
    ConversionWorker *m_worker = nullptr;
    QStringList m_logs;
    QString m_outputDirectory;
    bool m_isProcessing = false;
    QSettings m_settings;
};
