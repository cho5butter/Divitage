#include "MainWindow.h"

#include "AppState.h"
#include "ConverterPanel.h"
#include "HowToPanel.h"
#include "SettingsPanel.h"

#include <QAction>
#include <QApplication>
#include <QHBoxLayout>
#include <QListWidget>
#include <QShortcut>
#include <QStackedWidget>
#include <QSystemTrayIcon>
#include <QToolBar>

MainWindow::MainWindow(QWidget *parent)
    : QMainWindow(parent)
{
    m_state = new AppState(this);
    m_converter = new ConverterPanel(m_state, this);
    m_settings = new SettingsPanel(m_state, this);
    m_howTo = new HowToPanel(this);

    buildUi();
    setupTrayIcon();

    connect(m_state, &AppState::conversionCycleFinished, this, [this](const QString &message) {
        if (m_tray && QSystemTrayIcon::isSystemTrayAvailable()) {
            m_tray->showMessage(tr("Divitage"), message, QSystemTrayIcon::Information, 3000);
        }
    });
}

MainWindow::~MainWindow() = default;

void MainWindow::buildUi() {
    auto *central = new QWidget(this);
    auto *layout = new QHBoxLayout(central);
    layout->setContentsMargins(0, 0, 0, 0);

    m_navList = new QListWidget(this);
    m_navList->addItem(tr("コンバーター"));
    m_navList->addItem(tr("設定"));
    m_navList->addItem(tr("使い方"));
    m_navList->setFixedWidth(200);
    layout->addWidget(m_navList);

    m_stack = new QStackedWidget(this);
    m_stack->addWidget(m_converter);
    m_stack->addWidget(m_settings);
    m_stack->addWidget(m_howTo);
    layout->addWidget(m_stack, 1);

    setCentralWidget(central);
    setWindowTitle(tr("Divitage for Linux"));
    resize(1200, 720);

    connect(m_navList, &QListWidget::currentRowChanged, m_stack, &QStackedWidget::setCurrentIndex);
    m_navList->setCurrentRow(0);

    auto *toolbar = addToolBar(tr("MainToolbar"));
    toolbar->setMovable(false);
    auto *convertAction = toolbar->addAction(tr("変換"));
    connect(convertAction, &QAction::triggered, m_converter, &ConverterPanel::triggerConversion);

    auto *howToAction = toolbar->addAction(tr("ヘルプ"));
    connect(howToAction, &QAction::triggered, [this]() { m_navList->setCurrentRow(2); });

    auto *shortcut1 = new QShortcut(QKeySequence(Qt::CTRL | Qt::Key_1), this);
    connect(shortcut1, &QShortcut::activated, [this]() { m_navList->setCurrentRow(0); });
    auto *shortcut2 = new QShortcut(QKeySequence(Qt::CTRL | Qt::Key_2), this);
    connect(shortcut2, &QShortcut::activated, [this]() { m_navList->setCurrentRow(1); });
    auto *shortcut3 = new QShortcut(QKeySequence(Qt::CTRL | Qt::Key_3), this);
    connect(shortcut3, &QShortcut::activated, [this]() { m_navList->setCurrentRow(2); });
}

void MainWindow::setupTrayIcon() {
    if (!QSystemTrayIcon::isSystemTrayAvailable()) {
        return;
    }
    m_tray = new QSystemTrayIcon(windowIcon(), this);
    m_tray->setToolTip(tr("Divitage for Linux"));
    m_tray->show();
}
