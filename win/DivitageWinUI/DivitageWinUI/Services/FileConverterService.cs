using DivitageWinUI.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DivitageWinUI.Services;

/// <summary>
/// ファイル変換処理を行うサービスクラス
/// </summary>
public class FileConverterService
{
    /// <summary>
    /// 変換進捗が更新されたときに発生するイベント
    /// </summary>
    public event EventHandler<ConversionProgressEventArgs>? ProgressChanged;

    /// <summary>
    /// サポートされているメディアファイルの拡張子
    /// </summary>
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv", ".webm",
        ".mp3", ".wav", ".flac", ".aac", ".ogg", ".m4a",
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".tiff"
    };

    /// <summary>
    /// ファイルまたはフォルダを変換します
    /// </summary>
    /// <param name="sourcePath">変換元のパス</param>
    /// <param name="outputDirectory">出力先ディレクトリ</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>変換されたファイル数</returns>
    public async Task<int> ConvertAsync(string sourcePath, string outputDirectory, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            throw new ArgumentException("変換元のパスが指定されていません", nameof(sourcePath));
        }

        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            throw new ArgumentException("出力先ディレクトリが指定されていません", nameof(outputDirectory));
        }

        Directory.CreateDirectory(outputDirectory);

        if (File.Exists(sourcePath))
        {
            await ConvertFileAsync(sourcePath, outputDirectory, cancellationToken);
            return 1;
        }
        else if (Directory.Exists(sourcePath))
        {
            return await ConvertDirectoryAsync(sourcePath, outputDirectory, cancellationToken);
        }
        else
        {
            throw new FileNotFoundException($"指定されたパスが見つかりません: {sourcePath}");
        }
    }

    /// <summary>
    /// ディレクトリ内のファイルを再帰的に変換します
    /// </summary>
    private async Task<int> ConvertDirectoryAsync(string sourceDirectory, string outputDirectory, CancellationToken cancellationToken)
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

            await ConvertFileAsync(file, Path.GetDirectoryName(outputPath) ?? outputDirectory, cancellationToken);
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
    private async Task ConvertFileAsync(string sourceFile, string outputDirectory, CancellationToken cancellationToken)
    {
        var fileName = Path.GetFileName(sourceFile);
        var extension = Path.GetExtension(sourceFile);

        // サポートされていない拡張子はスキップ
        if (!SupportedExtensions.Contains(extension))
        {
            return;
        }

        var outputFile = Path.Combine(outputDirectory, fileName);
        Directory.CreateDirectory(outputDirectory);

        // TODO: 実際の変換処理（FFmpegなど）をここに実装
        // 現在は単純にファイルをコピーしています
        await Task.Run(() =>
        {
            File.Copy(sourceFile, outputFile, overwrite: true);

            // 元のファイルのタイムスタンプを保持
            if (SettingsHelper.PreserveTimestamp)
            {
                File.SetCreationTime(outputFile, File.GetCreationTime(sourceFile));
                File.SetLastWriteTime(outputFile, File.GetLastWriteTime(sourceFile));
            }
        }, cancellationToken);

        // 変換処理のシミュレーション（実際の処理では不要）
        await Task.Delay(100, cancellationToken);
    }

    /// <summary>
    /// 進捗変更イベントを発火します
    /// </summary>
    protected virtual void OnProgressChanged(ConversionProgressEventArgs e)
    {
        ProgressChanged?.Invoke(this, e);
    }
}

/// <summary>
/// 変換進捗情報を表すクラス
/// </summary>
public class ConversionProgressEventArgs : EventArgs
{
    /// <summary>
    /// 現在処理中のファイル名
    /// </summary>
    public string CurrentFile { get; set; } = string.Empty;

    /// <summary>
    /// 処理済みファイル数
    /// </summary>
    public int ProcessedCount { get; set; }

    /// <summary>
    /// 総ファイル数
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 完了パーセンテージ（0-100）
    /// </summary>
    public int PercentComplete { get; set; }
}
