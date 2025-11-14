#pragma once

#include <QThread>
#include <QString>

struct ConversionTask {
    QString sourcePath;
    QString destinationPath;
    QString displayName;
};

class ConversionWorker final : public QThread {
    Q_OBJECT
public:
    explicit ConversionWorker(const ConversionTask &task, QObject *parent = nullptr);

signals:
    void logMessage(const QString &message);
    void finished(bool success, const QString &errorMessage);

protected:
    void run() override;

private:
    ConversionTask m_task;
    bool copyRecursively(const QString &source, const QString &destination);
};

Q_DECLARE_METATYPE(ConversionTask)
