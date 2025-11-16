using DivitageWinUI.Helpers;
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
    private readonly FileConverterService _converterService = new();
    private bool _isProcessing;
    private CancellationTokenSource? _cancellationTokenSource;

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
        });
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

        _isProcessing = true;
        _cancellationTokenSource = new CancellationTokenSource();
        ConvertButton.IsEnabled = false;
        CancelButton.IsEnabled = true;
        StatusInfo.Text = "変換を実行中...";

        var snapshot = _pendingItems.ToList();
        _pendingItems.Clear();
        UpdateQueueText();

        try
        {
            int totalConverted = 0;

            foreach (var item in snapshot)
            {
                try
                {
                    var name = Path.GetFileName(item);
                    AppendLog($"変換開始: {name}");

                    var convertedCount = await _converterService.ConvertAsync(
                        item,
                        SettingsHelper.OutputDirectory,
                        _cancellationTokenSource.Token);

                    totalConverted += convertedCount;
                    AppendLog($"完了: {name} ({convertedCount} ファイル)");
                }
                catch (OperationCanceledException)
                {
                    AppendLog("変換がキャンセルされました");
                    StatusInfo.Text = "変換がキャンセルされました";
                    break;
                }
                catch (Exception ex)
                {
                    AppendLog($"エラー: {Path.GetFileName(item)} の変換に失敗しました - {ex.Message}");
                }
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
