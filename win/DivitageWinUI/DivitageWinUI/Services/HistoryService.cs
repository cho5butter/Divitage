using DivitageWinUI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DivitageWinUI.Services;

/// <summary>
/// 変換履歴の管理を行うサービスクラス
/// </summary>
public class HistoryService
{
    private readonly string _historyPath;
    private List<ConversionHistoryItem> _history = new();
    private const int MaxHistoryItems = 1000;

    public HistoryService()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Divitage"
        );
        Directory.CreateDirectory(appDataPath);
        _historyPath = Path.Combine(appDataPath, "history.json");
    }

    /// <summary>
    /// 履歴アイテムを追加します
    /// </summary>
    public async Task<ConversionHistoryItem> AddHistoryItemAsync(ConversionHistoryItem item)
    {
        await LoadHistoryAsync();

        item.Id = Guid.NewGuid().ToString();
        _history.Insert(0, item); // 最新のものを先頭に追加

        // 最大数を超えた場合は古いものを削除
        if (_history.Count > MaxHistoryItems)
        {
            _history = _history.Take(MaxHistoryItems).ToList();
        }

        await SaveHistoryAsync();
        return item;
    }

    /// <summary>
    /// すべての履歴を取得します
    /// </summary>
    public async Task<List<ConversionHistoryItem>> GetAllHistoryAsync()
    {
        await LoadHistoryAsync();
        return _history.ToList();
    }

    /// <summary>
    /// 期間を指定して履歴を取得します
    /// </summary>
    public async Task<List<ConversionHistoryItem>> GetHistoryByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        await LoadHistoryAsync();
        return _history
            .Where(h => h.StartTime >= startDate && h.StartTime <= endDate)
            .ToList();
    }

    /// <summary>
    /// 成功した変換のみを取得します
    /// </summary>
    public async Task<List<ConversionHistoryItem>> GetSuccessfulHistoryAsync()
    {
        await LoadHistoryAsync();
        return _history.Where(h => h.IsSuccess).ToList();
    }

    /// <summary>
    /// 失敗した変換のみを取得します
    /// </summary>
    public async Task<List<ConversionHistoryItem>> GetFailedHistoryAsync()
    {
        await LoadHistoryAsync();
        return _history.Where(h => !h.IsSuccess).ToList();
    }

    /// <summary>
    /// 履歴をクリアします
    /// </summary>
    public async Task ClearHistoryAsync()
    {
        _history.Clear();
        await SaveHistoryAsync();
    }

    /// <summary>
    /// 古い履歴を削除します
    /// </summary>
    public async Task DeleteOldHistoryAsync(int daysToKeep)
    {
        await LoadHistoryAsync();
        var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
        _history = _history.Where(h => h.StartTime >= cutoffDate).ToList();
        await SaveHistoryAsync();
    }

    /// <summary>
    /// 特定の履歴アイテムを削除します
    /// </summary>
    public async Task DeleteHistoryItemAsync(string historyId)
    {
        await LoadHistoryAsync();
        var item = _history.FirstOrDefault(h => h.Id == historyId);
        if (item != null)
        {
            _history.Remove(item);
            await SaveHistoryAsync();
        }
    }

    /// <summary>
    /// 履歴の統計情報を取得します
    /// </summary>
    public async Task<HistoryStatistics> GetStatisticsAsync()
    {
        await LoadHistoryAsync();

        return new HistoryStatistics
        {
            TotalConversions = _history.Count,
            SuccessfulConversions = _history.Count(h => h.IsSuccess),
            FailedConversions = _history.Count(h => !h.IsSuccess),
            TotalSourceSize = _history.Sum(h => h.SourceFileSize),
            TotalOutputSize = _history.Sum(h => h.OutputFileSize),
            AverageDuration = _history.Any()
                ? TimeSpan.FromSeconds(_history.Average(h => h.Duration.TotalSeconds))
                : TimeSpan.Zero,
            TotalDuration = TimeSpan.FromSeconds(_history.Sum(h => h.Duration.TotalSeconds))
        };
    }

    /// <summary>
    /// 履歴をファイルから読み込みます
    /// </summary>
    private async Task LoadHistoryAsync()
    {
        if (!File.Exists(_historyPath))
        {
            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_historyPath);
            var loadedHistory = JsonConvert.DeserializeObject<List<ConversionHistoryItem>>(json);
            if (loadedHistory != null)
            {
                _history = loadedHistory;
            }
        }
        catch
        {
            // 読み込みに失敗した場合は空のリストを使用
            _history = new List<ConversionHistoryItem>();
        }
    }

    /// <summary>
    /// 履歴をファイルに保存します
    /// </summary>
    private async Task SaveHistoryAsync()
    {
        try
        {
            var json = JsonConvert.SerializeObject(_history, Formatting.Indented);
            await File.WriteAllTextAsync(_historyPath, json);
        }
        catch
        {
            // 保存に失敗してもエラーにしない
        }
    }
}

/// <summary>
/// 履歴の統計情報
/// </summary>
public class HistoryStatistics
{
    public int TotalConversions { get; set; }
    public int SuccessfulConversions { get; set; }
    public int FailedConversions { get; set; }
    public long TotalSourceSize { get; set; }
    public long TotalOutputSize { get; set; }
    public TimeSpan AverageDuration { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public double SuccessRate => TotalConversions > 0
        ? (double)SuccessfulConversions / TotalConversions * 100
        : 0;
}
