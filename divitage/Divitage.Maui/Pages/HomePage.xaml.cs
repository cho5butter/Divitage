using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Divitage.Maui.Helpers;
using Divitage.Maui.Models;
using Divitage.Maui.Services;
using Microsoft.Maui.Storage;

namespace Divitage.Maui.Pages;

public partial class HomePage : ContentPage
{
    private readonly VideoSplitService _splitService;
    private readonly AppPreferences _preferences;
    private CancellationTokenSource? _cancellationTokenSource;

    public ObservableCollection<string> SelectedFiles { get; } = new();

    public HomePage()
        : this(ServiceHelper.GetService<VideoSplitService>(), ServiceHelper.GetService<AppPreferences>())
    {
    }

    public HomePage(VideoSplitService splitService, AppPreferences preferences)
    {
        InitializeComponent();
        _splitService = splitService;
        _preferences = preferences;
        BindingContext = this;
    }

    private async void OnPickFilesClicked(object sender, EventArgs e)
    {
        if (_cancellationTokenSource is { IsCancellationRequested: false })
        {
            await DisplayAlert("処理中", "現在の処理が完了するまでお待ちください", "OK");
            return;
        }

        try
        {
            var options = new PickOptions
            {
                PickerTitle = "動画ファイルを選択",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    [DevicePlatform.WinUI] = new[] { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".mpg", ".mkv" },
                    [DevicePlatform.MacCatalyst] = new[] { "public.movie" },
                    [DevicePlatform.iOS] = new[] { "public.movie" },
                    [DevicePlatform.Android] = new[] { "video/*" }
                })
            };

            var pickResult = await FilePicker.Default.PickMultipleAsync(options);
            if (pickResult is null)
            {
                return;
            }

            var filePaths = pickResult
                .Where(r => !string.IsNullOrWhiteSpace(r.FullPath))
                .Select(r => r.FullPath)
                .Distinct()
                .ToList();

            if (filePaths.Count == 0)
            {
                await DisplayAlert("ファイルなし", "処理可能なファイルが選択されませんでした", "OK");
                return;
            }

            SelectedFiles.Clear();
            foreach (var file in filePaths)
            {
                SelectedFiles.Add(Path.GetFileName(file));
            }

            await StartProcessingAsync(filePaths);
        }
        catch (Exception ex)
        {
            await DisplayAlert("エラー", ex.Message, "OK");
        }
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        _cancellationTokenSource?.Cancel();
    }

    private async Task StartProcessingAsync(IReadOnlyList<string> files)
    {
        Progress.Progress = 0;
        Status.Text = "処理を開始しています...";
        Summary.Text = string.Empty;
        Spinner.IsRunning = true;
        Spinner.IsVisible = true;
        CancelButton.IsEnabled = true;

        var settings = _preferences.ToSettings();
        Func<SplitPreview, Task<bool>>? confirm = null;
        if (settings.ConfirmBeforeSplit)
        {
            confirm = preview => SplitConfirmPage.RequestAsync(Navigation, preview);
        }

        _cancellationTokenSource = new CancellationTokenSource();
        var progress = new Progress<SplitProgress>(update =>
        {
            Progress.Progress = update.Percent / 100d;
            Status.Text = update.Message;
        });

        try
        {
            var summary = await _splitService.SplitAsync(files, settings, progress, confirm, _cancellationTokenSource.Token);
            Summary.Text = $"対象:{summary.RequestedFiles}件 / 完了:{summary.CompletedFiles}件 / スキップ:{summary.SkippedFiles}件";
            if (summary.Errors.Count > 0)
            {
                Summary.Text += "\n" + string.Join("\n", summary.Errors);
            }

            if (summary.CompletedFiles > 0)
            {
                await DisplayAlert("完了", $"{summary.CompletedFiles}件の分割が完了しました", "OK");
            }
            else if (summary.Errors.Count > 0)
            {
                await DisplayAlert("失敗", "一部のファイルでエラーが発生しました。詳細を確認してください。", "OK");
            }
            Status.Text = "処理が完了しました";
        }
        catch (OperationCanceledException)
        {
            Summary.Text = "処理を中断しました";
            Status.Text = "処理を中断しました";
        }
        catch (Exception ex)
        {
            Summary.Text = ex.Message;
            await DisplayAlert("エラー", ex.Message, "OK");
            Status.Text = "エラーが発生しました";
        }
        finally
        {
            Spinner.IsRunning = false;
            Spinner.IsVisible = false;
            CancelButton.IsEnabled = false;
            Progress.Progress = 0;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }
}
