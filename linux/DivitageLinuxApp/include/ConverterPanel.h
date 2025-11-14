#pragma once

#include <QWidget>
#include <QVector>

class AppState;
class QListWidget;
class QLabel;
class QPushButton;
class QTextEdit;
class DropArea;

class ConverterPanel final : public QWidget {
    Q_OBJECT
public:
    explicit ConverterPanel(AppState *state, QWidget *parent = nullptr);

private slots:
    void handleFilesDropped(const QStringList &paths);
    void pickFiles();
    void pickFolder();
    void chooseOutputDirectory();
    void triggerConversion();
    void updateQueueLabel(int queueLength);
    void updateOutputLabel(const QString &path);
    void updateProcessingState(bool isProcessing);

private:
    void buildUi();
    void addPendingPaths(const QStringList &paths);
    void syncConvertButton();

    AppState *m_state;
    DropArea *m_dropArea = nullptr;
    QListWidget *m_pendingList = nullptr;
    QLabel *m_pendingCountLabel = nullptr;
    QLabel *m_outputLabel = nullptr;
    QLabel *m_queueLabel = nullptr;
    QPushButton *m_convertButton = nullptr;
    QTextEdit *m_logView = nullptr;
    QVector<QString> m_pendingPaths;
    bool m_isProcessing = false;
};
