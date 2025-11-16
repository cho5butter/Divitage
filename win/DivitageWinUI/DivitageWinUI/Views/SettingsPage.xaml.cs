using DivitageWinUI.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;

namespace DivitageWinUI.Views;

/// <summary>
/// アプリケーション設定を管理するページ
/// </summary>
public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
        LoadSettings();

        // 出力ディレクトリの変更イベントを購読
        SettingsHelper.OutputDirectoryChanged += OnOutputDirectoryChanged;
    }

    private void LoadSettings()
    {
        // テーマ設定
        var theme = SettingsHelper.Theme;
        var themeButton = ThemeRadioButtons.Items[theme] as RadioButton;
        if (themeButton != null)
        {
            themeButton.IsChecked = true;
        }

        LaunchAtLoginToggle.IsOn = SettingsHelper.LaunchAtLogin;
        AutoCleanupToggle.IsOn = SettingsHelper.AutoCleanup;
        PreserveTimestampToggle.IsOn = SettingsHelper.PreserveTimestamp;
        OutputDirectoryText.Text = SettingsHelper.OutputDirectory;
        HistoryRetentionDaysBox.Value = SettingsHelper.HistoryRetentionDays;
    }

    private void OnOutputDirectoryChanged(object? sender, string newDirectory)
    {
        OutputDirectoryText.Text = newDirectory;
    }

    private void OnLaunchAtLoginToggled(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleSwitch toggle)
        {
            SettingsHelper.LaunchAtLogin = toggle.IsOn;
        }
    }

    private void OnAutoCleanupToggled(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleSwitch toggle)
        {
            SettingsHelper.AutoCleanup = toggle.IsOn;
        }
    }

    private void OnPreserveTimestampToggled(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleSwitch toggle)
        {
            SettingsHelper.PreserveTimestamp = toggle.IsOn;
        }
    }

    private void OnThemeSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ThemeRadioButtons.SelectedItem is RadioButton selectedButton &&
            selectedButton.Tag is string tagValue &&
            int.TryParse(tagValue, out int themeIndex))
        {
            SettingsHelper.Theme = themeIndex;
            ApplyTheme(themeIndex);
        }
    }

    private void ApplyTheme(int themeIndex)
    {
        if (App.MainWindow?.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = themeIndex switch
            {
                1 => ElementTheme.Light,
                2 => ElementTheme.Dark,
                _ => ElementTheme.Default
            };
        }
    }

    private void OnHistoryRetentionDaysChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (!double.IsNaN(args.NewValue))
        {
            SettingsHelper.HistoryRetentionDays = (int)args.NewValue;
        }
    }

    private async void OnRevealClicked(object sender, RoutedEventArgs e)
    {
        try
        {
            var path = SettingsHelper.OutputDirectory;
            if (Directory.Exists(path))
            {
                await Windows.System.Launcher.LaunchFolderPathAsync(path);
            }
        }
        catch (Exception ex)
        {
            // エラーハンドリング: ContentDialogなどで通知することも可能
            System.Diagnostics.Debug.WriteLine($"フォルダを開く際にエラーが発生しました: {ex.Message}");
        }
    }
}
