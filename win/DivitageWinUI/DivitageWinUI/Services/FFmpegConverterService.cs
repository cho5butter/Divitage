using DivitageWinUI.Helpers;
using DivitageWinUI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace DivitageWinUI.Services;

/// <summary>
/// FFmpegを使用したファイル変換処理を行うサービスクラス
/// </summary>
public class FFmpegConverterService
{
    private static bool _isFFmpegDownloaded = false;

    /// <summary>
    /// 変換進捗が更新されたときに発生するイベント
    /// </summary>
    public event EventHandler<ConversionProgressEventArgs>? ProgressChanged;

    /// <summary>
    /// サポートされているメディアファイルの拡張子
    /// </summary>
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv", ".webm", ".m4v", ".mpg", ".mpeg",
        ".mp3", ".wav", ".flac", ".aac", ".ogg", ".m4a", ".wma", ".opus",
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".tiff"
    };

    /// <summary>
    /// FFmpegの初期化を行います
    /// </summary>
    public static async Task InitializeAsync()
    {
        if (_isFFmpegDownloaded)
        {
            return;
        }

        try
        {
            var ffmpegPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Divitage",
                "ffmpeg"
            );

            Directory.CreateDirectory(ffmpegPath);

            if (!File.Exists(Path.Combine(ffmpegPath, "ffmpeg.exe")))
            {
                await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, ffmpegPath);
            }

            FFmpeg.SetExecutablesPath(ffmpegPath);
            _isFFmpegDownloaded = true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("FFmpegの初期化に失敗しました", ex);
        }
    }

    /// <summary>
    /// ファイルまたはフォルダを変換します
    /// </summary>
    /// <param name="sourcePath">変換元のパス</param>
    /// <param name="outputDirectory">出力先ディレクトリ</param>
    /// <param name="options">変換オプション</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>変換されたファイル数</returns>
    public async Task<int> ConvertAsync(
        string sourcePath,
        string outputDirectory,
        ConversionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            throw new ArgumentException("変換元のパスが指定されていません", nameof(sourcePath));
        }

        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            throw new ArgumentException("出力先ディレクトリが指定されていません", nameof(outputDirectory));
        }

        await InitializeAsync();

        options ??= ConversionOptions.CreateDefault();

        Directory.CreateDirectory(outputDirectory);

        if (File.Exists(sourcePath))
        {
            await ConvertFileAsync(sourcePath, outputDirectory, options, cancellationToken);
            return 1;
        }
        else if (Directory.Exists(sourcePath))
        {
            return await ConvertDirectoryAsync(sourcePath, outputDirectory, options, cancellationToken);
        }
        else
        {
            throw new FileNotFoundException($"指定されたパスが見つかりません: {sourcePath}");
        }
    }

    /// <summary>
    /// ディレクトリ内のファイルを再帰的に変換します
    /// </summary>
    private async Task<int> ConvertDirectoryAsync(
        string sourceDirectory,
        string outputDirectory,
        ConversionOptions options,
        CancellationToken cancellationToken)
    {
        int convertedCount = 0;
        var files = Directory.GetFiles(sourceDirectory, "*.*", SearchOption.AllDirectories)
            .Where(f => SupportedExtensions.Contains(Path.GetExtension(f)))
            .ToList();

        for (int i = 0; i < files.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var file = files[i];
            var relativePath = Path.GetRelativePath(sourceDirectory, file);
            var outputPath = Path.Combine(outputDirectory, relativePath);

            await ConvertFileAsync(file, Path.GetDirectoryName(outputPath) ?? outputDirectory, options, cancellationToken);
            convertedCount++;

            OnProgressChanged(new ConversionProgressEventArgs
            {
                CurrentFile = Path.GetFileName(file),
                ProcessedCount = i + 1,
                TotalCount = files.Count,
                PercentComplete = (int)((i + 1) / (double)files.Count * 100)
            });
        }

        return convertedCount;
    }

    /// <summary>
    /// 単一のファイルを変換します
    /// </summary>
    private async Task ConvertFileAsync(
        string sourceFile,
        string outputDirectory,
        ConversionOptions options,
        CancellationToken cancellationToken)
    {
        var fileName = Path.GetFileNameWithoutExtension(sourceFile);
        var extension = Path.GetExtension(sourceFile);

        // サポートされていない拡張子はスキップ
        if (!SupportedExtensions.Contains(extension))
        {
            return;
        }

        var outputFile = Path.Combine(outputDirectory, $"{fileName}.{options.OutputFormat}");
        Directory.CreateDirectory(outputDirectory);

        try
        {
            var mediaInfo = await FFmpeg.GetMediaInfo(sourceFile, cancellationToken);

            IConversion conversion = FFmpeg.Conversions.New();
            conversion.OnProgress += (sender, args) =>
            {
                var percent = (int)(args.Duration.TotalSeconds / args.TotalLength.TotalSeconds * 100);
                OnProgressChanged(new ConversionProgressEventArgs
                {
                    CurrentFile = Path.GetFileName(sourceFile),
                    ProcessedCount = 0,
                    TotalCount = 1,
                    PercentComplete = percent
                });
            };

            // ビデオストリームの処理
            if (!options.AudioOnly && mediaInfo.VideoStreams.Any())
            {
                var videoStream = mediaInfo.VideoStreams.First()
                    .SetCodec(options.VideoCodec);

                if (options.Width.HasValue && options.Height.HasValue)
                {
                    videoStream.SetSize(options.Width.Value, options.Height.Value);
                }

                if (options.FrameRate.HasValue)
                {
                    videoStream.SetFramerate(options.FrameRate.Value);
                }

                if (options.VideoBitrate.HasValue)
                {
                    videoStream.SetBitrate(options.VideoBitrate.Value * 1000);
                }

                conversion.AddStream(videoStream);
            }

            // オーディオストリームの処理
            if (!options.VideoOnly && mediaInfo.AudioStreams.Any())
            {
                var audioStream = mediaInfo.AudioStreams.First()
                    .SetCodec(options.AudioCodec);

                if (options.AudioBitrate.HasValue)
                {
                    audioStream.SetBitrate(options.AudioBitrate.Value * 1000);
                }

                conversion.AddStream(audioStream);
            }

            conversion.SetOutput(outputFile);

            // プリセットとCRFの設定
            conversion.SetPreset(ConvertPreset(options.Preset));

            if (options.CRF.HasValue && options.VideoCodec.Contains("264") || options.VideoCodec.Contains("265"))
            {
                conversion.AddParameter($"-crf {options.CRF.Value}");
            }

            // ハードウェアアクセラレーション
            if (options.UseHardwareAcceleration)
            {
                conversion.AddParameter("-hwaccel auto");
            }

            // カスタムパラメータ
            if (!string.IsNullOrWhiteSpace(options.CustomParameters))
            {
                conversion.AddParameter(options.CustomParameters);
            }

            await conversion.Start(cancellationToken);

            // タイムスタンプを保持
            if (SettingsHelper.PreserveTimestamp && File.Exists(outputFile))
            {
                File.SetCreationTime(outputFile, File.GetCreationTime(sourceFile));
                File.SetLastWriteTime(outputFile, File.GetLastWriteTime(sourceFile));
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"ファイルの変換に失敗しました: {sourceFile}", ex);
        }
    }

    /// <summary>
    /// プリセット文字列をXabe.FFmpegのConversionPresetに変換します
    /// </summary>
    private static ConversionPreset ConvertPreset(string preset)
    {
        return preset.ToLower() switch
        {
            "ultrafast" => ConversionPreset.UltraFast,
            "superfast" => ConversionPreset.SuperFast,
            "veryfast" => ConversionPreset.VeryFast,
            "faster" => ConversionPreset.Faster,
            "fast" => ConversionPreset.Fast,
            "medium" => ConversionPreset.Medium,
            "slow" => ConversionPreset.Slow,
            "slower" => ConversionPreset.Slower,
            "veryslow" => ConversionPreset.VerySlow,
            _ => ConversionPreset.Medium
        };
    }

    /// <summary>
    /// 進捗変更イベントを発火します
    /// </summary>
    protected virtual void OnProgressChanged(ConversionProgressEventArgs e)
    {
        ProgressChanged?.Invoke(this, e);
    }
}
