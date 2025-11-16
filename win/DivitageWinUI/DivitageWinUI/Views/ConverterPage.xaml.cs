using DivitageWinUI.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
    private bool _isProcessing;

    public ConverterPage()
    {
        InitializeComponent();
        PendingList.ItemsSource = _pendingItems;
        LogListView.ItemsSource = _logItems;

        // 設定から出力ディレクトリを取得
        OutputPathText.Text = SettingsHelper.OutputDirectory;

        // 出力ディレクトリの変更イベントを購読
        SettingsHelper.OutputDirectoryChanged += OnOutputDirectoryChanged;

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
        ConvertButton.IsEnabled = false;
        StatusInfo.Text = "変換を実行中...";

        var snapshot = _pendingItems.ToList();
        _pendingItems.Clear();
        UpdateQueueText();

        try
        {
            foreach (var item in snapshot)
            {
                try
                {
                    var name = Path.GetFileName(item);
                    AppendLog($"変換開始: {name}");

                    // TODO: 実際の変換処理をここに実装
                    await Task.Delay(350);

                    var destination = Path.Combine(SettingsHelper.OutputDirectory, name);
                    AppendLog($"完了: {destination}");
                }
                catch (Exception ex)
                {
                    AppendLog($"エラー: {Path.GetFileName(item)} の変換に失敗しました - {ex.Message}");
                }
            }

            StatusInfo.Text = "キューの処理が完了しました";
        }
        catch (Exception ex)
        {
            StatusInfo.Text = "処理中にエラーが発生しました";
            AppendLog($"エラー: {ex.Message}");
        }
        finally
        {
            _isProcessing = false;
            ConvertButton.IsEnabled = _pendingItems.Any();
        }
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
