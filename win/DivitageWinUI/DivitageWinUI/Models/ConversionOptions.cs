using System;

namespace DivitageWinUI.Models;

/// <summary>
/// メディアファイル変換のオプションを表すクラス
/// </summary>
public class ConversionOptions
{
    /// <summary>
    /// 出力フォーマット（拡張子）
    /// </summary>
    public string OutputFormat { get; set; } = "mp4";

    /// <summary>
    /// ビデオコーデック
    /// </summary>
    public string VideoCodec { get; set; } = "libx264";

    /// <summary>
    /// オーディオコーデック
    /// </summary>
    public string AudioCodec { get; set; } = "aac";

    /// <summary>
    /// ビデオビットレート（kbps）
    /// </summary>
    public int? VideoBitrate { get; set; }

    /// <summary>
    /// オーディオビットレート（kbps）
    /// </summary>
    public int? AudioBitrate { get; set; } = 128;

    /// <summary>
    /// フレームレート
    /// </summary>
    public int? FrameRate { get; set; }

    /// <summary>
    /// 解像度の幅
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// 解像度の高さ
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// 品質プリセット（ultrafast, superfast, veryfast, faster, fast, medium, slow, slower, veryslow）
    /// </summary>
    public string Preset { get; set; } = "medium";

    /// <summary>
    /// CRF品質値（0-51、低いほど高品質、推奨18-28）
    /// </summary>
    public int? CRF { get; set; } = 23;

    /// <summary>
    /// 2パスエンコーディングを使用するか
    /// </summary>
    public bool TwoPassEncoding { get; set; } = false;

    /// <summary>
    /// ハードウェアアクセラレーションを使用するか
    /// </summary>
    public bool UseHardwareAcceleration { get; set; } = true;

    /// <summary>
    /// オーディオのみ抽出するか
    /// </summary>
    public bool AudioOnly { get; set; } = false;

    /// <summary>
    /// ビデオのみ抽出するか
    /// </summary>
    public bool VideoOnly { get; set; } = false;

    /// <summary>
    /// 追加のFFmpegパラメータ
    /// </summary>
    public string? CustomParameters { get; set; }

    /// <summary>
    /// デフォルトオプションを作成します
    /// </summary>
    public static ConversionOptions CreateDefault() => new();

    /// <summary>
    /// 高品質プリセット
    /// </summary>
    public static ConversionOptions CreateHighQuality() => new()
    {
        Preset = "slow",
        CRF = 18,
        AudioBitrate = 192
    };

    /// <summary>
    /// 高速プリセット
    /// </summary>
    public static ConversionOptions CreateFast() => new()
    {
        Preset = "veryfast",
        CRF = 28,
        AudioBitrate = 128
    };

    /// <summary>
    /// Web用プリセット
    /// </summary>
    public static ConversionOptions CreateWebOptimized() => new()
    {
        OutputFormat = "mp4",
        VideoCodec = "libx264",
        AudioCodec = "aac",
        Preset = "fast",
        CRF = 23,
        Width = 1280,
        Height = 720
    };

    /// <summary>
    /// オプションのクローンを作成します
    /// </summary>
    public ConversionOptions Clone() => (ConversionOptions)MemberwiseClone();
}
