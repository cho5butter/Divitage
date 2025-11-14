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

public sealed partial class ConverterPage : Page
{
    private readonly ObservableCollection<string> _pendingItems = new();
    private readonly ObservableCollection<string> _logItems = new();
    private bool _isProcessing;
    private string _outputDirectory;

    public ConverterPage()
    {
        InitializeComponent();
        PendingList.ItemsSource = _pendingItems;
        LogListView.ItemsSource = _logItems;

        var downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        _outputDirectory = Path.Combine(downloads, "DivitageOutput");
        Directory.CreateDirectory(_outputDirectory);
        OutputPathText.Text = _outputDirectory;

        var accelerator = new KeyboardAccelerator { Key = Windows.System.VirtualKey.Enter, Modifiers = Windows.System.VirtualKeyModifiers.Control };
        accelerator.Invoked += (_, _) => { OnConvertClicked(null!, null!); };
        KeyboardAccelerators.Add(accelerator);
    }

    private async void OnAddFilesClicked(object sender, RoutedEventArgs e)
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

    private async void OnAddFolderClicked(object sender, RoutedEventArgs e)
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

    private async void OnChangeOutputClicked(object sender, RoutedEventArgs e)
    {
        var picker = new FolderPicker();
        picker.FileTypeFilter.Add("*");
        InitializeWithWindow.Initialize(picker, WindowHelper.Handle);
        StorageFolder folder = await picker.PickSingleFolderAsync();
        if (folder is not null)
        {
            _outputDirectory = folder.Path;
            OutputPathText.Text = _outputDirectory;
        }
    }

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

    private void UpdateQueueText()
    {
        PendingCountText.Text = $"{_pendingItems.Count} 件";
        QueueStatusText.Text = $"キュー: {_pendingItems.Count} 件";
        ConvertButton.IsEnabled = !_isProcessing && _pendingItems.Any();
    }

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

        foreach (var item in snapshot)
        {
            var name = Path.GetFileName(item);
            AppendLog($"変換開始: {name}");
            await Task.Delay(350);
            var destination = Path.Combine(_outputDirectory, name);
            AppendLog($"完了: {destination}");
        }

        _isProcessing = false;
        StatusInfo.Text = "キューの処理が完了しました";
        ConvertButton.IsEnabled = _pendingItems.Any();
    }

    private void AppendLog(string message)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}] {message}";
        _logItems.Add(line);
    }

    private void OnDropZoneDragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
    }

    private async void OnDropZoneDrop(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
        var items = await e.DataView.GetStorageItemsAsync();
        foreach (var storageItem in items)
        {
            AddPending(storageItem.Path);
        }
    }
}
