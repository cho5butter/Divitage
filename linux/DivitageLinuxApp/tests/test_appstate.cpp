#include <QtTest/QtTest>
#include <QTemporaryDir>
#include <QFile>
#include "AppState.h"

class TestAppState : public QObject {
    Q_OBJECT

private slots:
    void initTestCase() {
        // Set up test environment
    }

    void testInitialization() {
        AppState state;
        QVERIFY(!state.outputDirectory().isEmpty());
        QVERIFY(state.logs().isEmpty());
    }

    void testSetOutputDirectory() {
        AppState state;
        QTemporaryDir tempDir;
        QVERIFY(tempDir.isValid());

        QString testDir = tempDir.path() + "/test_output";
        QSignalSpy spy(&state, &AppState::outputDirectoryChanged);

        state.setOutputDirectory(testDir);

        QCOMPARE(state.outputDirectory(), testDir);
        QCOMPARE(spy.count(), 1);
        QVERIFY(QDir(testDir).exists());
    }

    void testEnqueueSources() {
        AppState state;
        QTemporaryDir tempDir;
        QVERIFY(tempDir.isValid());

        // Create test files
        QString file1 = tempDir.path() + "/test1.txt";
        QString file2 = tempDir.path() + "/test2.txt";

        QFile f1(file1);
        QVERIFY(f1.open(QIODevice::WriteOnly));
        f1.write("test content 1");
        f1.close();

        QFile f2(file2);
        QVERIFY(f2.open(QIODevice::WriteOnly));
        f2.write("test content 2");
        f2.close();

        QSignalSpy queueSpy(&state, &AppState::queueLengthChanged);
        QSignalSpy logSpy(&state, &AppState::logAppended);

        QStringList paths = {file1, file2};
        int added = state.enqueueSources(paths);

        QCOMPARE(added, 2);
        QCOMPARE(queueSpy.count(), 1);
        QVERIFY(logSpy.count() > 0);
    }

    void testEnqueueNonExistentFile() {
        AppState state;
        QSignalSpy queueSpy(&state, &AppState::queueLengthChanged);
        QSignalSpy logSpy(&state, &AppState::logAppended);

        QStringList paths = {"/non/existent/file.txt"};
        int added = state.enqueueSources(paths);

        QCOMPARE(added, 0);
        QCOMPARE(queueSpy.count(), 0);
        QVERIFY(logSpy.count() > 0); // Should log error message
    }

    void testSettingValue() {
        AppState state;

        state.setSettingValue("testKey", true);
        QVERIFY(state.settingValue("testKey", false));

        state.setSettingValue("testKey", false);
        QVERIFY(!state.settingValue("testKey", true));
    }

    void testSettingDefaultValue() {
        AppState state;

        // Test non-existent key returns default
        QVERIFY(state.settingValue("nonExistentKey", true));
        QVERIFY(!state.settingValue("nonExistentKey", false));
    }

    void cleanupTestCase() {
        // Clean up after tests
    }
};

QTEST_MAIN(TestAppState)
#include "test_appstate.moc"
