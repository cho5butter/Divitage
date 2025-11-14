using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using Windows.Storage;

namespace DivitageWinUI.Views;

public sealed partial class SettingsPage : Page
{
    private readonly ApplicationDataContainer _settings = ApplicationData.Current.LocalSettings;

    public SettingsPage()
    {
        InitializeComponent();
        var downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        OutputDirectoryText.Text = Path.Combine(downloads, "DivitageOutput");
    }

    private void OnLaunchAtLoginToggled(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleSwitch toggle)
        {
            _settings.Values["launchAtLogin"] = toggle.IsOn;
        }
    }

    private void OnAutoCleanupToggled(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleSwitch toggle)
        {
            _settings.Values["autoCleanup"] = toggle.IsOn;
        }
    }

    private void OnPreserveTimestampToggled(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleSwitch toggle)
        {
            _settings.Values["preserveTimestamp"] = toggle.IsOn;
        }
    }

    private async void OnRevealClicked(object sender, RoutedEventArgs e)
    {
        var path = OutputDirectoryText.Text;
        if (Directory.Exists(path))
        {
            await Windows.System.Launcher.LaunchFolderPathAsync(path);
        }
    }
}
