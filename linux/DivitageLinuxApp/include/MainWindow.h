#pragma once

#include <QMainWindow>

class AppState;
class ConverterPanel;
class SettingsPanel;
class HowToPanel;
class QListWidget;
class QStackedWidget;
class QSystemTrayIcon;

class MainWindow final : public QMainWindow {
    Q_OBJECT
public:
    explicit MainWindow(QWidget *parent = nullptr);
    ~MainWindow() override;

private:
    void buildUi();
    void setupTrayIcon();

    AppState *m_state = nullptr;
    ConverterPanel *m_converter = nullptr;
    SettingsPanel *m_settings = nullptr;
    HowToPanel *m_howTo = nullptr;
    QListWidget *m_navList = nullptr;
    QStackedWidget *m_stack = nullptr;
    QSystemTrayIcon *m_tray = nullptr;
};
