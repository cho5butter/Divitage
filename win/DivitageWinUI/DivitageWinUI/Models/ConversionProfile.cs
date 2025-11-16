using System;

namespace DivitageWinUI.Models;

/// <summary>
/// 保存可能な変換プロファイル
/// </summary>
public class ConversionProfile
{
    /// <summary>
    /// プロファイルID
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// プロファイル名
    /// </summary>
    public string Name { get; set; } = "新しいプロファイル";

    /// <summary>
    /// プロファイルの説明
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 変換オプション
    /// </summary>
    public ConversionOptions Options { get; set; } = new();

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 最終更新日時
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// お気に入りフラグ
    /// </summary>
    public bool IsFavorite { get; set; }
}
