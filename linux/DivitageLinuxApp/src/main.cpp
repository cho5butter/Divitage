#include "MainWindow.h"

#include <QApplication>

int main(int argc, char *argv[]) {
    QApplication app(argc, argv);
    QApplication::setOrganizationName("Divitage");
    QApplication::setApplicationName("Divitage");

    MainWindow window;
    window.show();

    return QApplication::exec();
}
