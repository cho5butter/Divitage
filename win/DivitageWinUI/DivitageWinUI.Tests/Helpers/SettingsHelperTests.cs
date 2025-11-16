using DivitageWinUI.Helpers;
using System;
using System.IO;
using Xunit;

namespace DivitageWinUI.Tests.Helpers;

/// <summary>
/// SettingsHelperクラスのテスト
/// </summary>
public class SettingsHelperTests
{
    [Fact]
    public void OutputDirectory_ShouldReturnDefaultPath_WhenNotSet()
    {
        // Act
        var outputDirectory = SettingsHelper.OutputDirectory;

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(outputDirectory));
        Assert.Contains("DivitageOutput", outputDirectory);
    }

    [Fact]
    public void OutputDirectory_ShouldRaiseEvent_WhenChanged()
    {
        // Arrange
        var eventRaised = false;
        var newPath = Path.Combine(Path.GetTempPath(), "TestOutput");
        EventHandler<string>? handler = (sender, path) =>
        {
            eventRaised = true;
            Assert.Equal(newPath, path);
        };

        SettingsHelper.OutputDirectoryChanged += handler;

        try
        {
            // Act
            SettingsHelper.OutputDirectory = newPath;

            // Assert
            Assert.True(eventRaised);
            Assert.Equal(newPath, SettingsHelper.OutputDirectory);
        }
        finally
        {
            SettingsHelper.OutputDirectoryChanged -= handler;

            // Cleanup
            if (Directory.Exists(newPath))
            {
                try { Directory.Delete(newPath); } catch { }
            }
        }
    }

    [Fact]
    public void LaunchAtLogin_ShouldDefaultToFalse()
    {
        // Act & Assert
        Assert.False(SettingsHelper.LaunchAtLogin);
    }

    [Fact]
    public void AutoCleanup_ShouldDefaultToTrue()
    {
        // Act & Assert
        Assert.True(SettingsHelper.AutoCleanup);
    }

    [Fact]
    public void PreserveTimestamp_ShouldDefaultToTrue()
    {
        // Act & Assert
        Assert.True(SettingsHelper.PreserveTimestamp);
    }

    [Fact]
    public void Settings_ShouldPersist_WhenChanged()
    {
        // Arrange
        var originalValue = SettingsHelper.LaunchAtLogin;

        try
        {
            // Act
            SettingsHelper.LaunchAtLogin = true;

            // Assert
            Assert.True(SettingsHelper.LaunchAtLogin);

            // Act again
            SettingsHelper.LaunchAtLogin = false;

            // Assert again
            Assert.False(SettingsHelper.LaunchAtLogin);
        }
        finally
        {
            // Restore original value
            SettingsHelper.LaunchAtLogin = originalValue;
        }
    }
}
