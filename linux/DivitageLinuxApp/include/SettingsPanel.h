#pragma once

#include <QWidget>

class AppState;
class QLabel;
class QCheckBox;

class SettingsPanel final : public QWidget {
    Q_OBJECT
public:
    explicit SettingsPanel(AppState *state, QWidget *parent = nullptr);

private slots:
    void chooseOutputDirectory();
    void toggleLaunchAtLogin(bool enabled);
    void toggleAutoCleanup(bool enabled);
    void togglePreserveTimestamps(bool enabled);

private:
    void buildUi();
    void bindSettings();

    AppState *m_state;
    QLabel *m_outputLabel = nullptr;
    QCheckBox *m_launchToggle = nullptr;
    QCheckBox *m_cleanupToggle = nullptr;
    QCheckBox *m_preserveToggle = nullptr;
};
