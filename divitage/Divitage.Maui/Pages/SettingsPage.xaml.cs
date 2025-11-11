using Divitage.Maui.Helpers;
using Divitage.Maui.Models;
using Divitage.Maui.Services;
using Microsoft.Maui.Storage;

namespace Divitage.Maui.Pages;

public partial class SettingsPage : ContentPage
{
    private readonly AppPreferences _preferences;
    private bool _isInitializing;

    private static readonly string[] ImageFormats =
    {
        "JPEG (.jpg)",
        "BMP (.bmp)",
        "TIFF (.tif)",
        "PNG (.png)",
        "GIF (.gif)"
    };

    private static readonly string[] NameConventions =
    {
        "元ファイル名",
        "日付 + 元ファイル名",
        "時間 + 元ファイル名",
        "日付 + 時間 + 元ファイル名",
        "ランダム + 元ファイル名"
    };

    private static readonly string[] SplitModes =
    {
        "指定フレームごと",
        "指定％ごと"
    };

    public SettingsPage()
        : this(ServiceHelper.GetService<AppPreferences>())
    {
    }

    public SettingsPage(AppPreferences preferences)
    {
        InitializeComponent();
        _preferences = preferences;

        ImageFormatPicker.ItemsSource = ImageFormats;
        NameConventionPicker.ItemsSource = NameConventions;
        SplitModePicker.ItemsSource = SplitModes;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadPreferences();
    }

    private void LoadPreferences()
    {
        _isInitializing = true;
        SaveLocationPicker.SelectedIndex = (int)_preferences.SaveLocation;
        DirectoryEntry.Text = _preferences.CustomDirectory;
        DirectoryEntry.IsEnabled = _preferences.SaveLocation == SaveLocationOption.CustomDirectory;
        ConfirmSwitch.IsToggled = _preferences.ConfirmBeforeSplit;
        ImageFormatPicker.SelectedIndex = (int)_preferences.ImageFormat;
        NameConventionPicker.SelectedIndex = (int)_preferences.NameConvention;
        SplitModePicker.SelectedIndex = (int)_preferences.Mode;
        SplitIntervalEntry.Text = _preferences.SplitInterval.ToString();
        FrameRangeSwitch.IsToggled = _preferences.RestrictFrameRange;
        StartFrameEntry.Text = _preferences.StartFrame.ToString();
        EndFrameEntry.Text = _preferences.EndFrame.ToString();
        StartFrameEntry.IsEnabled = FrameRangeSwitch.IsToggled;
        EndFrameEntry.IsEnabled = FrameRangeSwitch.IsToggled;
        StatusLabel.Text = "";
        _isInitializing = false;
    }

    private void OnSaveLocationChanged(object sender, EventArgs e)
    {
        if (_isInitializing) return;
        _preferences.SaveLocation = (SaveLocationOption)SaveLocationPicker.SelectedIndex;
        DirectoryEntry.IsEnabled = _preferences.SaveLocation == SaveLocationOption.CustomDirectory;
        UpdateStatus();
    }

    private async void OnBrowseClicked(object sender, EventArgs e)
    {
        if (!FolderPicker.Default.IsSupported)
        {
            await DisplayAlert("未対応", "このプラットフォームではフォルダー選択に対応していません。手動で入力してください。", "OK");
            return;
        }

        var result = await FolderPicker.Default.PickAsync();
        if (result is null) return;

        DirectoryEntry.Text = result.Folder?.Path ?? string.Empty;
        _preferences.CustomDirectory = DirectoryEntry.Text;
        _preferences.SaveLocation = SaveLocationOption.CustomDirectory;
        SaveLocationPicker.SelectedIndex = (int)SaveLocationOption.CustomDirectory;
        DirectoryEntry.IsEnabled = true;
        UpdateStatus();
    }

    private void OnConfirmToggled(object sender, ToggledEventArgs e)
    {
        if (_isInitializing) return;
        _preferences.ConfirmBeforeSplit = e.Value;
        UpdateStatus();
    }

    private void OnDirectoryChanged(object sender, TextChangedEventArgs e)
    {
        if (_isInitializing) return;
        _preferences.CustomDirectory = DirectoryEntry.Text ?? string.Empty;
        UpdateStatus();
    }

    private void OnImageFormatChanged(object sender, EventArgs e)
    {
        if (_isInitializing) return;
        _preferences.ImageFormat = (ImageFormatOption)ImageFormatPicker.SelectedIndex;
        UpdateStatus();
    }

    private void OnNameConventionChanged(object sender, EventArgs e)
    {
        if (_isInitializing) return;
        _preferences.NameConvention = (NameConventionOption)NameConventionPicker.SelectedIndex;
        UpdateStatus();
    }

    private void OnSplitModeChanged(object sender, EventArgs e)
    {
        if (_isInitializing) return;
        _preferences.Mode = (SplitMode)SplitModePicker.SelectedIndex;
        UpdateStatus();
    }

    private void OnSplitIntervalChanged(object sender, TextChangedEventArgs e)
    {
        if (_isInitializing) return;
        if (int.TryParse(SplitIntervalEntry.Text, out var value) && value > 0)
        {
            _preferences.SplitInterval = value;
            UpdateStatus();
        }
    }

    private void OnFrameRangeToggled(object sender, ToggledEventArgs e)
    {
        if (_isInitializing) return;
        _preferences.RestrictFrameRange = e.Value;
        StartFrameEntry.IsEnabled = e.Value;
        EndFrameEntry.IsEnabled = e.Value;
        UpdateStatus();
    }

    private void OnStartFrameChanged(object sender, TextChangedEventArgs e)
    {
        if (_isInitializing) return;
        if (int.TryParse(StartFrameEntry.Text, out var value))
        {
            _preferences.StartFrame = value;
            UpdateStatus();
        }
    }

    private void OnEndFrameChanged(object sender, TextChangedEventArgs e)
    {
        if (_isInitializing) return;
        if (int.TryParse(EndFrameEntry.Text, out var value))
        {
            _preferences.EndFrame = value;
            UpdateStatus();
        }
    }

    private void UpdateStatus()
    {
        StatusLabel.Text = $"保存しました ({DateTime.Now:HH:mm:ss})";
    }
}
