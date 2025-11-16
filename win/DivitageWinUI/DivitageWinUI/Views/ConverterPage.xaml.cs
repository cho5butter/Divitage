using DivitageWinUI.Helpers;
using DivitageWinUI.Models;
using DivitageWinUI.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace DivitageWinUI.Views;

/// <summary>
/// メディアファイルの変換を行うメインページ
/// </summary>
public sealed partial class ConverterPage : Page
{
    private readonly ObservableCollection<string> _pendingItems = new();
    private readonly ObservableCollection<string> _logItems = new();
    private readonly FFmpegConverterService _converterService = new();
    private readonly ProfileService _profileService = new();
    private readonly HistoryService _historyService = new();
    private bool _isProcessing;
    private CancellationTokenSource? _cancellationTokenSource;
    private List<ConversionProfile> _profiles = new();
    private ConversionProfile? _selectedProfile;

    public ConverterPage()
    {
        InitializeComponent();
        PendingList.ItemsSource = _pendingItems;
        LogListView.ItemsSource = _logItems;

        // 設定から出力ディレクトリを取得
        OutputPathText.Text = SettingsHelper.OutputDirectory;

        // 出力ディレクトリの変更イベントを購読
        SettingsHelper.OutputDirectoryChanged += OnOutputDirectoryChanged;

        // 変換進捗イベントを購読
        _converterService.ProgressChanged += OnConversionProgressChanged;

        var accelerator = new KeyboardAccelerator { Key = Windows.System.VirtualKey.Enter, Modifiers = Windows.System.VirtualKeyModifiers.Control };
        accelerator.Invoked += (_, _) => { OnConvertClicked(null!, null!); };
        KeyboardAccelerators.Add(accelerator);

        // プロファイルを読み込み
        LoadProfilesAsync();

        // FFmpegを初期化
        InitializeFFmpegAsync();
    }

    /// <summary>
    /// プロファイルを読み込みます
    /// </summary>
    private async void LoadProfilesAsync()
    {
        try
        {
            _profiles = await _profileService.GetAllProfilesAsync();
            ProfileComboBox.ItemsSource = _profiles;

            // 保存されているプロファイルを選択
            var savedProfileId = SettingsHelper.SelectedProfileId;
            if (!string.IsNullOrEmpty(savedProfileId))
            {
                _selectedProfile = _profiles.FirstOrDefault(p => p.Id == savedProfileId);
            }

            // プロファイルが見つからない場合は最初のプロファイルを選択
            if (_selectedProfile == null && _profiles.Any())
            {
                _selectedProfile = _profiles[0];
            }

            ProfileComboBox.SelectedItem = _selectedProfile;
        }
        catch (Exception ex)
        {
            AppendLog($"エラー: プロファイルの読み込みに失敗しました - {ex.Message}");
        }
    }

    /// <summary>
    /// FFmpegを初期化します
    /// </summary>
    private async void InitializeFFmpegAsync()
    {
        try
        {
            AppendLog("FFmpegを初期化中...");
            await FFmpegConverterService.InitializeAsync();
            AppendLog("FFmpegの初期化が完了しました");
        }
        catch (Exception ex)
        {
            AppendLog($"エラー: FFmpegの初期化に失敗しました - {ex.Message}");
        }
    }

    /// <summary>
    /// 出力ディレクトリが変更されたときの処理
    /// </summary>
    private void OnOutputDirectoryChanged(object? sender, string newDirectory)
    {
        OutputPathText.Text = newDirectory;
    }

    /// <summary>
    /// 変換進捗が変更されたときの処理
    /// </summary>
    private void OnConversionProgressChanged(object? sender, ConversionProgressEventArgs e)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            StatusInfo.Text = $"変換中... ({e.ProcessedCount}/{e.TotalCount}) - {e.PercentComplete}%";
            ConversionProgressBar.Value = e.PercentComplete;
            ProgressPercentText.Text = $"{e.PercentComplete}%";
            CurrentFileText.Text = $"現在の処理: {e.CurrentFile}";
        });
    }

    /// <summary>
    /// プロファイルが選択されたときの処理
    /// </summary>
    private void OnProfileSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedProfile = ProfileComboBox.SelectedItem as ConversionProfile;
        if (_selectedProfile != null)
        {
            SettingsHelper.SelectedProfileId = _selectedProfile.Id;
            AppendLog($"プロファイルを変更: {_selectedProfile.Name}");
        }
    }

    /// <summary>
    /// プロファイル設定ボタンがクリックされたときの処理
    /// </summary>
    private async void OnConfigureProfileClicked(object sender, RoutedEventArgs e)
    {
        if (_selectedProfile == null)
        {
            return;
        }

        var dialog = new ContentDialog
        {
            Title = $"プロファイル設定: {_selectedProfile.Name}",
            Content = CreateProfileConfigContent(_selectedProfile.Options),
            PrimaryButtonText = "保存",
            CloseButtonText = "キャンセル",
            XamlRoot = XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            await _profileService.UpdateProfileAsync(_selectedProfile);
            AppendLog($"プロファイル設定を保存しました: {_selectedProfile.Name}");
        }
    }

    /// <summary>
    /// プロファイル設定用のUIを作成します
    /// </summary>
    private UIElement CreateProfileConfigContent(ConversionOptions options)
    {
        var stackPanel = new StackPanel { Spacing = 12 };

        // 出力フォーマット
        stackPanel.Children.Add(new TextBlock { Text = "出力フォーマット", FontWeight = new Windows.UI.Text.FontWeight { Weight = 600 } });
        var formatBox = new TextBox { Text = options.OutputFormat, PlaceholderText = "mp4, mkv, avi など" };
        formatBox.TextChanged += (s, e) => options.OutputFormat = formatBox.Text;
        stackPanel.Children.Add(formatBox);

        // プリセット
        stackPanel.Children.Add(new TextBlock { Text = "品質プリセット", FontWeight = new Windows.UI.Text.FontWeight { Weight = 600 }, Margin = new Thickness(0, 8, 0, 0) });
        var presetCombo = new ComboBox { SelectedItem = options.Preset };
        presetCombo.Items.Add("ultrafast");
        presetCombo.Items.Add("superfast");
        presetCombo.Items.Add("veryfast");
        presetCombo.Items.Add("faster");
        presetCombo.Items.Add("fast");
        presetCombo.Items.Add("medium");
        presetCombo.Items.Add("slow");
        presetCombo.Items.Add("slower");
        presetCombo.Items.Add("veryslow");
        presetCombo.SelectedItem = options.Preset;
        presetCombo.SelectionChanged += (s, e) => options.Preset = presetCombo.SelectedItem?.ToString() ?? "medium";
        stackPanel.Children.Add(presetCombo);

        return new ScrollViewer { Content = stackPanel, MaxHeight = 400 };
    }

    /// <summary>
    /// プロファイル管理ボタンがクリックされたときの処理
    /// </summary>
    private async void OnManageProfilesClicked(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "プロファイル管理",
            Content = new TextBlock { Text = "プロファイル管理機能は今後実装予定です。" },
            CloseButtonText = "閉じる",
            XamlRoot = XamlRoot
        };

        await dialog.ShowAsync();
    }

    /// <summary>
    /// 履歴ボタンがクリックされたときの処理
    /// </summary>
    private async void OnHistoryClicked(object sender, RoutedEventArgs e)
    {
        try
        {
            var history = await _historyService.GetAllHistoryAsync();
            var stats = await _historyService.GetStatisticsAsync();

            var content = new StackPanel { Spacing = 12 };
            content.Children.Add(new TextBlock
            {
                Text = $"総変換数: {stats.TotalConversions}",
                FontWeight = new Windows.UI.Text.FontWeight { Weight = 600 }
            });
            content.Children.Add(new TextBlock { Text = $"成功: {stats.SuccessfulConversions}" });
            content.Children.Add(new TextBlock { Text = $"失敗: {stats.FailedConversions}" });
            content.Children.Add(new TextBlock { Text = $"成功率: {stats.SuccessRate:F1}%" });

            var listView = new ListView { MaxHeight = 300 };
            listView.ItemsSource = history.Take(50).Select(h =>
                $"{h.StartTime:yyyy/MM/dd HH:mm} - {h.SourceFileName} → {h.OutputFileName} ({(h.IsSuccess ? "成功" : "失敗")})"
            ).ToList();
            content.Children.Add(listView);

            var dialog = new ContentDialog
            {
                Title = "変換履歴",
                Content = new ScrollViewer { Content = content, MaxHeight = 500 },
                CloseButtonText = "閉じる",
                XamlRoot = XamlRoot
            };

            await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            AppendLog($"エラー: 履歴の表示に失敗しました - {ex.Message}");
        }
    }

    /// <summary>
    /// ファイル追加ボタンがクリックされたときの処理
    /// </summary>
    private async void OnAddFilesClicked(object sender, RoutedEventArgs e)
    {
        try
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add("*");
            InitializeWithWindow.Initialize(picker, WindowHelper.Handle);
            var files = await picker.PickMultipleFilesAsync();
            if (files is null)
            {
                return;
            }
            foreach (var file in files)
            {
                AddPending(file.Path);
            }
        }
        catch (Exception ex)
        {
            AppendLog($"エラー: ファイルの追加に失敗しました - {ex.Message}");
        }
    }

    /// <summary>
    /// フォルダ追加ボタンがクリックされたときの処理
    /// </summary>
    private async void OnAddFolderClicked(object sender, RoutedEventArgs e)
    {
        try
        {
            var picker = new FolderPicker();
            picker.FileTypeFilter.Add("*");
            InitializeWithWindow.Initialize(picker, WindowHelper.Handle);
            StorageFolder folder = await picker.PickSingleFolderAsync();
            if (folder is not null)
            {
                AddPending(folder.Path);
            }
        }
        catch (Exception ex)
        {
            AppendLog($"エラー: フォルダの追加に失敗しました - {ex.Message}");
        }
    }

    /// <summary>
    /// 出力先変更ボタンがクリックされたときの処理
    /// </summary>
    private async void OnChangeOutputClicked(object sender, RoutedEventArgs e)
    {
        try
        {
            var picker = new FolderPicker();
            picker.FileTypeFilter.Add("*");
            InitializeWithWindow.Initialize(picker, WindowHelper.Handle);
            StorageFolder folder = await picker.PickSingleFolderAsync();
            if (folder is not null)
            {
                SettingsHelper.OutputDirectory = folder.Path;
            }
        }
        catch (Exception ex)
        {
            AppendLog($"エラー: 出力先の変更に失敗しました - {ex.Message}");
        }
    }

    /// <summary>
    /// 変換キューにパスを追加します
    /// </summary>
    /// <param name="path">追加するファイルまたはフォルダのパス</param>
    private void AddPending(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }
        if (_pendingItems.Contains(path))
        {
            return;
        }
        _pendingItems.Add(path);
        UpdateQueueText();
    }

    /// <summary>
    /// キューの表示を更新します
    /// </summary>
    private void UpdateQueueText()
    {
        PendingCountText.Text = $"{_pendingItems.Count} 件";
        QueueStatusText.Text = $"キュー: {_pendingItems.Count} 件";
        ConvertButton.IsEnabled = !_isProcessing && _pendingItems.Any();
    }

    /// <summary>
    /// 変換開始ボタンがクリックされたときの処理
    /// </summary>
    private async void OnConvertClicked(object? sender, RoutedEventArgs? e)
    {
        if (_isProcessing || !_pendingItems.Any())
        {
            return;
        }

        if (_selectedProfile == null)
        {
            AppendLog("エラー: 変換プロファイルが選択されていません");
            return;
        }

        _isProcessing = true;
        _cancellationTokenSource = new CancellationTokenSource();
        ConvertButton.IsEnabled = false;
        CancelButton.IsEnabled = true;
        ProgressSection.Visibility = Visibility.Visible;
        StatusInfo.Text = "変換を実行中...";

        var snapshot = _pendingItems.ToList();
        _pendingItems.Clear();
        UpdateQueueText();

        try
        {
            int totalConverted = 0;

            foreach (var item in snapshot)
            {
                var historyItem = new ConversionHistoryItem
                {
                    SourcePath = item,
                    ProfileName = _selectedProfile.Name,
                    StartTime = DateTime.Now
                };

                try
                {
                    var name = Path.GetFileName(item);
                    AppendLog($"変換開始: {name}");

                    if (File.Exists(item))
                    {
                        historyItem.SourceFileSize = new FileInfo(item).Length;
                    }

                    var convertedCount = await _converterService.ConvertAsync(
                        item,
                        SettingsHelper.OutputDirectory,
                        _selectedProfile.Options,
                        _cancellationTokenSource.Token);

                    totalConverted += convertedCount;
                    historyItem.EndTime = DateTime.Now;
                    historyItem.IsSuccess = true;
                    historyItem.OutputPath = Path.Combine(
                        SettingsHelper.OutputDirectory,
                        Path.GetFileNameWithoutExtension(item) + "." + _selectedProfile.Options.OutputFormat
                    );

                    if (File.Exists(historyItem.OutputPath))
                    {
                        historyItem.OutputFileSize = new FileInfo(historyItem.OutputPath).Length;
                    }

                    AppendLog($"完了: {name} ({convertedCount} ファイル)");
                }
                catch (OperationCanceledException)
                {
                    historyItem.EndTime = DateTime.Now;
                    historyItem.IsSuccess = false;
                    historyItem.ErrorMessage = "キャンセルされました";
                    AppendLog("変換がキャンセルされました");
                    StatusInfo.Text = "変換がキャンセルされました";
                    await _historyService.AddHistoryItemAsync(historyItem);
                    break;
                }
                catch (Exception ex)
                {
                    historyItem.EndTime = DateTime.Now;
                    historyItem.IsSuccess = false;
                    historyItem.ErrorMessage = ex.Message;
                    AppendLog($"エラー: {Path.GetFileName(item)} の変換に失敗しました - {ex.Message}");
                }

                await _historyService.AddHistoryItemAsync(historyItem);
            }

            if (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                StatusInfo.Text = $"キューの処理が完了しました（{totalConverted} ファイル変換）";

                // 自動クリーンアップが有効な場合、一時ファイルを削除
                if (SettingsHelper.AutoCleanup)
                {
                    AppendLog("一時ファイルをクリーンアップ中...");
                    // TODO: 一時ファイルのクリーンアップ処理
                }
            }
        }
        catch (Exception ex)
        {
            StatusInfo.Text = "処理中にエラーが発生しました";
            AppendLog($"エラー: {ex.Message}");
        }
        finally
        {
            _isProcessing = false;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            ConvertButton.IsEnabled = _pendingItems.Any();
            CancelButton.IsEnabled = false;
            ProgressSection.Visibility = Visibility.Collapsed;
        }
    }

    /// <summary>
    /// キャンセルボタンがクリックされたときの処理
    /// </summary>
    private void OnCancelClicked(object sender, RoutedEventArgs e)
    {
        _cancellationTokenSource?.Cancel();
        CancelButton.IsEnabled = false;
        AppendLog("変換のキャンセルを要求しました...");
    }

    /// <summary>
    /// ログにメッセージを追加します
    /// </summary>
    /// <param name="message">追加するメッセージ</param>
    private void AppendLog(string message)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}] {message}";
        _logItems.Add(line);

        // 最新のログアイテムまでスクロール
        if (_logItems.Count > 0)
        {
            LogListView.ScrollIntoView(_logItems[^1]);
        }
    }

    /// <summary>
    /// ドロップゾーンにファイルがドラッグされたときの処理
    /// </summary>
    private void OnDropZoneDragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
    }

    /// <summary>
    /// ドロップゾーンにファイルがドロップされたときの処理
    /// </summary>
    private async void OnDropZoneDrop(object sender, DragEventArgs e)
    {
        try
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            var items = await e.DataView.GetStorageItemsAsync();
            foreach (var storageItem in items)
            {
                AddPending(storageItem.Path);
            }
        }
        catch (Exception ex)
        {
            AppendLog($"エラー: ドロップ操作に失敗しました - {ex.Message}");
        }
    }
}
