using DivitageWinUI.Services;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DivitageWinUI.Tests.Services;

/// <summary>
/// FileConverterServiceクラスのテスト
/// </summary>
public class FileConverterServiceTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly string _outputDirectory;
    private readonly FileConverterService _service;

    public FileConverterServiceTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"DivitageTest_{Guid.NewGuid()}");
        _outputDirectory = Path.Combine(Path.GetTempPath(), $"DivitageOutput_{Guid.NewGuid()}");
        _service = new FileConverterService();

        Directory.CreateDirectory(_testDirectory);
        Directory.CreateDirectory(_outputDirectory);
    }

    [Fact]
    public async Task ConvertAsync_ShouldThrowException_WhenSourcePathIsEmpty()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _service.ConvertAsync("", _outputDirectory));
    }

    [Fact]
    public async Task ConvertAsync_ShouldThrowException_WhenOutputDirectoryIsEmpty()
    {
        // Arrange
        var testFile = CreateTestFile("test.mp4");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _service.ConvertAsync(testFile, ""));
    }

    [Fact]
    public async Task ConvertAsync_ShouldThrowException_WhenSourcePathDoesNotExist()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testDirectory, "nonexistent.mp4");

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(async () =>
            await _service.ConvertAsync(nonExistentPath, _outputDirectory));
    }

    [Fact]
    public async Task ConvertAsync_ShouldConvertSingleFile_Successfully()
    {
        // Arrange
        var testFile = CreateTestFile("test.mp4");

        // Act
        var result = await _service.ConvertAsync(testFile, _outputDirectory);

        // Assert
        Assert.Equal(1, result);
        var outputFile = Path.Combine(_outputDirectory, "test.mp4");
        Assert.True(File.Exists(outputFile));
    }

    [Fact]
    public async Task ConvertAsync_ShouldConvertDirectory_Successfully()
    {
        // Arrange
        CreateTestFile("video1.mp4");
        CreateTestFile("video2.avi");
        CreateTestFile("audio.mp3");

        // Act
        var result = await _service.ConvertAsync(_testDirectory, _outputDirectory);

        // Assert
        Assert.Equal(3, result);
        Assert.True(File.Exists(Path.Combine(_outputDirectory, "video1.mp4")));
        Assert.True(File.Exists(Path.Combine(_outputDirectory, "video2.avi")));
        Assert.True(File.Exists(Path.Combine(_outputDirectory, "audio.mp3")));
    }

    [Fact]
    public async Task ConvertAsync_ShouldSkipUnsupportedFiles()
    {
        // Arrange
        CreateTestFile("supported.mp4");
        CreateTestFile("unsupported.txt");

        // Act
        var result = await _service.ConvertAsync(_testDirectory, _outputDirectory);

        // Assert
        Assert.Equal(1, result);
        Assert.True(File.Exists(Path.Combine(_outputDirectory, "supported.mp4")));
        Assert.False(File.Exists(Path.Combine(_outputDirectory, "unsupported.txt")));
    }

    [Fact]
    public async Task ConvertAsync_ShouldRaiseProgressEvents()
    {
        // Arrange
        CreateTestFile("file1.mp4");
        CreateTestFile("file2.mp4");
        var progressEventRaised = false;

        _service.ProgressChanged += (sender, e) =>
        {
            progressEventRaised = true;
            Assert.True(e.ProcessedCount > 0);
            Assert.True(e.TotalCount > 0);
            Assert.True(e.PercentComplete >= 0 && e.PercentComplete <= 100);
        };

        // Act
        await _service.ConvertAsync(_testDirectory, _outputDirectory);

        // Assert
        Assert.True(progressEventRaised);
    }

    [Fact]
    public async Task ConvertAsync_ShouldRespectCancellationToken()
    {
        // Arrange
        CreateTestFile("file1.mp4");
        CreateTestFile("file2.mp4");
        CreateTestFile("file3.mp4");

        var cts = new CancellationTokenSource();
        cts.CancelAfter(100); // Cancel after 100ms

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await _service.ConvertAsync(_testDirectory, _outputDirectory, cts.Token));
    }

    private string CreateTestFile(string fileName)
    {
        var filePath = Path.Combine(_testDirectory, fileName);
        File.WriteAllText(filePath, "Test content");
        return filePath;
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }
        catch { }

        try
        {
            if (Directory.Exists(_outputDirectory))
            {
                Directory.Delete(_outputDirectory, true);
            }
        }
        catch { }
    }
}
