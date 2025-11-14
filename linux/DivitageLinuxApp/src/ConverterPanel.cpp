#include "ConverterPanel.h"

#include "AppState.h"
#include "DropArea.h"

#include <QFileDialog>
#include <QHBoxLayout>
#include <QGridLayout>
#include <QLabel>
#include <QListWidget>
#include <QPushButton>
#include <QShortcut>
#include <QTextEdit>
#include <QVBoxLayout>

ConverterPanel::ConverterPanel(AppState *state, QWidget *parent)
    : QWidget(parent)
    , m_state(state)
{
    buildUi();
    for (const QString &line : m_state->logs()) {
        m_logView->append(line);
    }

    connect(m_state, &AppState::logAppended, m_logView, &QTextEdit::append);
    connect(m_state, &AppState::queueLengthChanged, this, &ConverterPanel::updateQueueLabel);
    connect(m_state, &AppState::outputDirectoryChanged, this, &ConverterPanel::updateOutputLabel);
    connect(m_state, &AppState::processingStateChanged, this, &ConverterPanel::updateProcessingState);

    updateOutputLabel(m_state->outputDirectory());
    updateQueueLabel(0);
    updateProcessingState(false);

    auto shortcut = new QShortcut(QKeySequence(Qt::CTRL | Qt::Key_Return), this);
    connect(shortcut, &QShortcut::activated, this, &ConverterPanel::triggerConversion);
    auto shortcutAlt = new QShortcut(QKeySequence(Qt::CTRL | Qt::Key_Enter), this);
    connect(shortcutAlt, &QShortcut::activated, this, &ConverterPanel::triggerConversion);
}

void ConverterPanel::buildUi() {
    auto *root = new QVBoxLayout(this);
    root->setSpacing(16);

    auto *title = new QLabel(tr("<h2>Divitage for Linux</h2>"
                                "<p>ファイルやフォルダを投入し、変換キューを作成します。</p>"));
    title->setWordWrap(true);
    root->addWidget(title);

    m_dropArea = new DropArea(this);
    root->addWidget(m_dropArea);
    connect(m_dropArea, &DropArea::filesDropped, this, &ConverterPanel::handleFilesDropped);

    auto *actionsRow = new QHBoxLayout();
    auto *fileButton = new QPushButton(tr("ファイルを選択"));
    connect(fileButton, &QPushButton::clicked, this, &ConverterPanel::pickFiles);
    auto *folderButton = new QPushButton(tr("フォルダを選択"));
    connect(folderButton, &QPushButton::clicked, this, &ConverterPanel::pickFolder);
    actionsRow->addWidget(fileButton);
    actionsRow->addWidget(folderButton);
    actionsRow->addStretch();
    root->addLayout(actionsRow);

    auto *pendingHeader = new QHBoxLayout();
    pendingHeader->addWidget(new QLabel(tr("投入予定")));
    m_pendingCountLabel = new QLabel(tr("0 件"));
    pendingHeader->addWidget(m_pendingCountLabel);
    pendingHeader->addStretch();
    root->addLayout(pendingHeader);

    m_pendingList = new QListWidget(this);
    m_pendingList->setAlternatingRowColors(true);
    root->addWidget(m_pendingList);

    auto *outputRow = new QHBoxLayout();
    outputRow->addWidget(new QLabel(tr("出力先:")));
    m_outputLabel = new QLabel(this);
    m_outputLabel->setWordWrap(true);
    outputRow->addWidget(m_outputLabel, 1);
    auto *outputButton = new QPushButton(tr("変更"));
    connect(outputButton, &QPushButton::clicked, this, &ConverterPanel::chooseOutputDirectory);
    outputRow->addWidget(outputButton);
    root->addLayout(outputRow);

    auto *queueRow = new QHBoxLayout();
    m_queueLabel = new QLabel(tr("キュー: 0 件"));
    queueRow->addWidget(m_queueLabel);
    queueRow->addStretch();
    m_convertButton = new QPushButton(tr("変換開始"));
    connect(m_convertButton, &QPushButton::clicked, this, &ConverterPanel::triggerConversion);
    queueRow->addWidget(m_convertButton);
    root->addLayout(queueRow);

    root->addWidget(new QLabel(tr("アクティビティログ")));
    m_logView = new QTextEdit(this);
    m_logView->setReadOnly(true);
    m_logView->setLineWrapMode(QTextEdit::NoWrap);
    root->addWidget(m_logView, 1);
}

void ConverterPanel::handleFilesDropped(const QStringList &paths) {
    addPendingPaths(paths);
}

void ConverterPanel::pickFiles() {
    const QStringList files = QFileDialog::getOpenFileNames(this, tr("変換するファイルを選択"));
    addPendingPaths(files);
}

void ConverterPanel::pickFolder() {
    const QString folder = QFileDialog::getExistingDirectory(this, tr("フォルダを選択"));
    if (!folder.isEmpty()) {
        addPendingPaths(QStringList{folder});
    }
}

void ConverterPanel::chooseOutputDirectory() {
    const QString directory = QFileDialog::getExistingDirectory(this, tr("出力先ディレクトリ"), m_state->outputDirectory());
    if (!directory.isEmpty()) {
        m_state->setOutputDirectory(directory);
    }
}

void ConverterPanel::triggerConversion() {
    if (m_pendingPaths.isEmpty()) {
        return;
    }
    QStringList list;
    for (const QString &path : std::as_const(m_pendingPaths)) {
        list << path;
    }
    const int added = m_state->enqueueSources(list);
    if (added > 0) {
        m_state->startProcessing();
        m_pendingPaths.clear();
        m_pendingList->clear();
        m_pendingCountLabel->setText(tr("0 件"));
    }
    syncConvertButton();
}

void ConverterPanel::updateQueueLabel(int queueLength) {
    m_queueLabel->setText(tr("キュー: %1 件").arg(queueLength));
}

void ConverterPanel::updateOutputLabel(const QString &path) {
    m_outputLabel->setText(path);
}

void ConverterPanel::updateProcessingState(bool isProcessing) {
    m_isProcessing = isProcessing;
    if (isProcessing) {
        m_convertButton->setText(tr("処理中..."));
    } else {
        m_convertButton->setText(tr("変換開始"));
    }
    syncConvertButton();
}

void ConverterPanel::addPendingPaths(const QStringList &paths) {
    bool added = false;
    for (const QString &path : paths) {
        if (path.isEmpty()) {
            continue;
        }
        if (m_pendingPaths.contains(path)) {
            continue;
        }
        m_pendingPaths.append(path);
        m_pendingList->addItem(path);
        added = true;
    }
    if (added) {
        m_pendingCountLabel->setText(tr("%1 件").arg(m_pendingPaths.size()));
        m_pendingList->scrollToBottom();
    }
    syncConvertButton();
}

void ConverterPanel::syncConvertButton() {
    m_convertButton->setEnabled(!m_pendingPaths.isEmpty() && !m_isProcessing);
}
