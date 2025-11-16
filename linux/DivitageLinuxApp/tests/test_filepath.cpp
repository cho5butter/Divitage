#include <QtTest/QtTest>
#include <QTemporaryDir>
#include <QFileInfo>
#include <QDir>

class TestFilePath : public QObject {
    Q_OBJECT

private slots:
    void testFilePathConstruction() {
        QTemporaryDir tempDir;
        QVERIFY(tempDir.isValid());

        QString testPath = tempDir.path() + "/test.txt";
        QFileInfo info(testPath);

        QCOMPARE(info.fileName(), QString("test.txt"));
        QCOMPARE(info.completeBaseName(), QString("test"));
        QCOMPARE(info.suffix(), QString("txt"));
    }

    void testFilePathWithMultipleDots() {
        QFileInfo info("/path/to/file.tar.gz");

        QCOMPARE(info.fileName(), QString("file.tar.gz"));
        QCOMPARE(info.baseName(), QString("file"));
        QCOMPARE(info.completeBaseName(), QString("file.tar"));
        QCOMPARE(info.suffix(), QString("gz"));
        QCOMPARE(info.completeSuffix(), QString("tar.gz"));
    }

    void testDirectoryPath() {
        QTemporaryDir tempDir;
        QVERIFY(tempDir.isValid());

        QString subDir = tempDir.path() + "/subdir";
        QDir dir;
        QVERIFY(dir.mkpath(subDir));
        QVERIFY(QDir(subDir).exists());
    }

    void testPathJoin() {
        QDir dir("/base/path");
        QString joined = dir.filePath("file.txt");

        QCOMPARE(joined, QString("/base/path/file.txt"));
    }

    void testAbsolutePath() {
        QTemporaryDir tempDir;
        QVERIFY(tempDir.isValid());

        QString relativePath = "test.txt";
        QDir dir(tempDir.path());
        QString absolutePath = dir.absoluteFilePath(relativePath);

        QVERIFY(QFileInfo(absolutePath).isAbsolute());
        QVERIFY(absolutePath.contains(tempDir.path()));
    }

    void testFileExists() {
        QTemporaryDir tempDir;
        QVERIFY(tempDir.isValid());

        QString filePath = tempDir.path() + "/exists.txt";
        QFile file(filePath);

        QVERIFY(!QFileInfo::exists(filePath));

        QVERIFY(file.open(QIODevice::WriteOnly));
        file.write("content");
        file.close();

        QVERIFY(QFileInfo::exists(filePath));
    }
};

QTEST_MAIN(TestFilePath)
#include "test_filepath.moc"
