#include "SettingsPanel.h"

#include "AppState.h"

#include <QCheckBox>
#include <QFileDialog>
#include <QFormLayout>
#include <QGroupBox>
#include <QHBoxLayout>
#include <QLabel>
#include <QPushButton>
#include <QVBoxLayout>

SettingsPanel::SettingsPanel(AppState *state, QWidget *parent)
    : QWidget(parent)
    , m_state(state)
{
    buildUi();
    bindSettings();
}

void SettingsPanel::buildUi() {
    auto *root = new QVBoxLayout(this);
    root->setSpacing(18);

    auto *generalGroup = new QGroupBox(tr("一般"));
    auto *form = new QFormLayout(generalGroup);

    m_launchToggle = new QCheckBox(tr("ログイン時に起動"));
    connect(m_launchToggle, &QCheckBox::toggled, this, &SettingsPanel::toggleLaunchAtLogin);
    form->addRow(m_launchToggle);

    m_cleanupToggle = new QCheckBox(tr("処理後に一時ファイルを削除"));
    connect(m_cleanupToggle, &QCheckBox::toggled, this, &SettingsPanel::toggleAutoCleanup);
    form->addRow(m_cleanupToggle);

    m_preserveToggle = new QCheckBox(tr("タイムスタンプを保持"));
    connect(m_preserveToggle, &QCheckBox::toggled, this, &SettingsPanel::togglePreserveTimestamps);
    form->addRow(m_preserveToggle);
    root->addWidget(generalGroup);

    auto *outputGroup = new QGroupBox(tr("出力"));
    auto *outputLayout = new QVBoxLayout(outputGroup);
    outputLayout->addWidget(new QLabel(tr("保存先ディレクトリ")));
    m_outputLabel = new QLabel(m_state->outputDirectory());
    m_outputLabel->setWordWrap(true);
    outputLayout->addWidget(m_outputLabel);

    auto *buttonRow = new QHBoxLayout();
    auto *chooseButton = new QPushButton(tr("変更"));
    connect(chooseButton, &QPushButton::clicked, this, &SettingsPanel::chooseOutputDirectory);
    buttonRow->addWidget(chooseButton);
    buttonRow->addStretch();
    outputLayout->addLayout(buttonRow);
    root->addWidget(outputGroup);

    root->addStretch();
}

void SettingsPanel::bindSettings() {
    if (m_launchToggle) {
        m_launchToggle->setChecked(m_state->settingValue("launchAtLogin", false));
    }
    if (m_cleanupToggle) {
        m_cleanupToggle->setChecked(m_state->settingValue("autoCleanup", true));
    }
    if (m_preserveToggle) {
        m_preserveToggle->setChecked(m_state->settingValue("preserveTimestamps", true));
    }
}

void SettingsPanel::chooseOutputDirectory() {
    const QString directory = QFileDialog::getExistingDirectory(this, tr("出力先を選択"), m_state->outputDirectory());
    if (!directory.isEmpty()) {
        m_state->setOutputDirectory(directory);
        m_outputLabel->setText(directory);
    }
}

void SettingsPanel::toggleLaunchAtLogin(bool enabled) {
    m_state->setSettingValue("launchAtLogin", enabled);
}

void SettingsPanel::toggleAutoCleanup(bool enabled) {
    m_state->setSettingValue("autoCleanup", enabled);
}

void SettingsPanel::togglePreserveTimestamps(bool enabled) {
    m_state->setSettingValue("preserveTimestamps", enabled);
}
