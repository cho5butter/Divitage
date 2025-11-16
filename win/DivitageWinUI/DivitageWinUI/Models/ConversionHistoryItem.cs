using System;

namespace DivitageWinUI.Models;

/// <summary>
/// 変換履歴アイテム
/// </summary>
public class ConversionHistoryItem
{
    /// <summary>
    /// 履歴ID
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// ソースファイルパス
    /// </summary>
    public string SourcePath { get; set; } = string.Empty;

    /// <summary>
    /// 出力ファイルパス
    /// </summary>
    public string OutputPath { get; set; } = string.Empty;

    /// <summary>
    /// 変換開始日時
    /// </summary>
    public DateTime StartTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 変換終了日時
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 変換が成功したか
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// エラーメッセージ（失敗時）
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 使用されたプロファイル名
    /// </summary>
    public string? ProfileName { get; set; }

    /// <summary>
    /// 元のファイルサイズ（バイト）
    /// </summary>
    public long SourceFileSize { get; set; }

    /// <summary>
    /// 出力ファイルサイズ（バイト）
    /// </summary>
    public long OutputFileSize { get; set; }

    /// <summary>
    /// 変換にかかった時間
    /// </summary>
    public TimeSpan Duration => EndTime.HasValue ? EndTime.Value - StartTime : TimeSpan.Zero;

    /// <summary>
    /// 表示用のソースファイル名
    /// </summary>
    public string SourceFileName => System.IO.Path.GetFileName(SourcePath);

    /// <summary>
    /// 表示用の出力ファイル名
    /// </summary>
    public string OutputFileName => System.IO.Path.GetFileName(OutputPath);
}
