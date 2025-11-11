using System.IO;
using System.Linq;
using Divitage.Maui.Models;
using OpenCvSharp;

namespace Divitage.Maui.Services;

public sealed class VideoSplitService
{
    private static readonly string[] AllowedExtensions = { ".avi", ".mp4", ".mov", ".wmv", ".flv", ".mpg", ".mkv" };

    public async Task<SplitSummary> SplitAsync(
        IEnumerable<string> files,
        SplitSettings settings,
        IProgress<SplitProgress>? progress = null,
        Func<SplitPreview, Task<bool>>? confirm = null,
        CancellationToken cancellationToken = default)
    {
        if (files is null) throw new ArgumentNullException(nameof(files));
        var fileQueue = files
            .Where(f => !string.IsNullOrWhiteSpace(f))
            .Select(f => f.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var requestedCount = fileQueue.Count;

        var eligibleFiles = new List<string>();
        var errors = new List<string>();

        foreach (var path in fileQueue)
        {
            if (!File.Exists(path))
            {
                errors.Add($"ファイルが見つかりません: {path}");
                continue;
            }

            var extension = Path.GetExtension(path).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                errors.Add($"非対応のファイル形式です: {Path.GetFileName(path)} ({extension})");
                continue;
            }

            eligibleFiles.Add(path);
        }

        if (eligibleFiles.Count == 0)
        {
            return new SplitSummary(requestedCount, 0, 0, errors);
        }

        var completed = 0;
        var skipped = 0;

        for (int i = 0; i < eligibleFiles.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var targetFile = eligibleFiles[i];
            try
            {
                var result = await Task.Run(() =>
                    ProcessFile(targetFile, settings, i + 1, eligibleFiles.Count, progress, confirm, cancellationToken), cancellationToken);

                if (result.Success)
                {
                    completed++;
                }
                else if (result.Skipped)
                {
                    skipped++;
                    if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
                    {
                        errors.Add(result.ErrorMessage);
                    }
                }
                else if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
                {
                    errors.Add(result.ErrorMessage);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                errors.Add($"{Path.GetFileName(targetFile)} の処理でエラーが発生しました: {ex.Message}");
            }
        }

        progress?.Report(new SplitProgress(100, "すべての処理が完了しました"));
        return new SplitSummary(requestedCount, completed, skipped, errors);
    }

    private FileSplitResult ProcessFile(
        string filePath,
        SplitSettings settings,
        int fileIndex,
        int totalFiles,
        IProgress<SplitProgress>? progress,
        Func<SplitPreview, Task<bool>>? confirm,
        CancellationToken cancellationToken)
    {
        using var capture = new VideoCapture(filePath);
        if (!capture.IsOpened())
        {
            return FileSplitResult.Failed($"{Path.GetFileName(filePath)} を開けませんでした");
        }

        var totalFrames = (int)capture.FrameCount;
        if (totalFrames <= 0)
        {
            return FileSplitResult.Skipped($"{Path.GetFileName(filePath)} にフレームがありません");
        }

        var destinationFolder = CreateDestinationFolder(filePath, settings);
        var extension = ResolveExtension(settings.ImageFormat);
        var savedFrames = 0;
        var confirmationSatisfied = !settings.ConfirmBeforeSplit;

        try
        {
            for (int frameIndex = 0; frameIndex < totalFrames; frameIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!ShouldUseFrame(frameIndex, totalFrames, settings))
                {
                    continue;
                }

                capture.PosFrames = frameIndex;
                using var frame = new Mat();
                if (!capture.Read(frame) || frame.Empty())
                {
                    continue;
                }

                if (!confirmationSatisfied)
                {
                    var previewBytes = Cv2.ImEncode(".png", frame);
                    var preview = new SplitPreview(
                        filePath,
                        totalFrames,
                        new FileInfo(filePath).Length,
                        frameIndex + 1,
                        previewBytes);

                    confirmationSatisfied = confirm?.Invoke(preview).GetAwaiter().GetResult() ?? true;
                    if (!confirmationSatisfied)
                    {
                        DeleteDirectorySafe(destinationFolder);
                        return FileSplitResult.Skipped($"{Path.GetFileName(filePath)} の処理がキャンセルされました");
                    }
                }

                var outputName = Path.Combine(destinationFolder, $"{frameIndex + 1}.{extension}");
                frame.SaveImage(outputName);
                savedFrames++;

                var percent = CalculateProgressPercent(fileIndex, totalFiles, frameIndex + 1, totalFrames);
                var message = $"{Path.GetFileName(filePath)} を処理中 ({frameIndex + 1}/{totalFrames})";
                progress?.Report(new SplitProgress(percent, message));
            }
        }
        catch
        {
            DeleteDirectorySafe(destinationFolder);
            throw;
        }
        finally
        {
            capture.Release();
        }

        if (savedFrames == 0)
        {
            DeleteDirectorySafe(destinationFolder);
            return FileSplitResult.Skipped($"{Path.GetFileName(filePath)} は指定条件に一致しませんでした");
        }

        return FileSplitResult.Successful();
    }

    private static double CalculateProgressPercent(int fileIndex, int totalFiles, int currentFrame, int totalFrames)
    {
        var filePortion = 1d / totalFiles;
        var frameProgress = currentFrame / (double)totalFrames;
        return Math.Min(100, ((fileIndex - 1) * filePortion + frameProgress * filePortion) * 100);
    }

    private static string ResolveExtension(ImageFormatOption format) => format switch
    {
        ImageFormatOption.Jpeg => "jpg",
        ImageFormatOption.Bmp => "bmp",
        ImageFormatOption.Tiff => "tif",
        ImageFormatOption.Png => "png",
        ImageFormatOption.Gif => "gif",
        _ => "jpg"
    };

    private static bool ShouldUseFrame(int frameIndex, int totalFrames, SplitSettings settings)
    {
        var frameNumber = frameIndex + 1;
        if (settings.RestrictFrameRange)
        {
            if (settings.StartFrame > 0 && frameNumber < settings.StartFrame)
            {
                return false;
            }

            if (settings.EndFrame > 0 && frameNumber > settings.EndFrame)
            {
                return false;
            }
        }

        if (settings.Mode == SplitMode.FrameInterval)
        {
            return frameIndex % Math.Max(1, settings.SplitInterval) == 0;
        }

        var intervalPercent = Math.Clamp(settings.SplitInterval, 1, 100);
        var intervalFrames = Math.Max(1, (int)((totalFrames / 100.0) * intervalPercent));
        return frameIndex % intervalFrames == 0;
    }

    private static string CreateDestinationFolder(string filePath, SplitSettings settings)
    {
        var sourceDirectory = Path.GetDirectoryName(filePath) ?? Environment.CurrentDirectory;
        var baseDirectory = settings.SaveLocation == SaveLocationOption.SameDirectory || string.IsNullOrWhiteSpace(settings.CustomDirectory)
            ? sourceDirectory
            : settings.CustomDirectory!;

        Directory.CreateDirectory(baseDirectory);

        var now = DateTime.Now;
        var originalName = Path.GetFileNameWithoutExtension(filePath);
        var folderName = settings.NameConvention switch
        {
            NameConventionOption.Original => originalName,
            NameConventionOption.DateOriginal => $"{now:yyyyMMdd}_{originalName}",
            NameConventionOption.TimeOriginal => $"{now:HHmm}_{originalName}",
            NameConventionOption.DateTimeOriginal => $"{now:yyyyMMdd_HHmm}_{originalName}",
            _ => $"{Guid.NewGuid():N}_{originalName}"
        };

        var destination = Path.Combine(baseDirectory, folderName);
        var counter = 1;
        while (Directory.Exists(destination))
        {
            destination = Path.Combine(baseDirectory, $"{folderName}_{counter++}");
        }

        Directory.CreateDirectory(destination);
        return destination;
    }

    private static void DeleteDirectorySafe(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
        catch
        {
            // best effort cleanup
        }
    }

    private sealed record FileSplitResult(bool Success, bool Skipped, string? ErrorMessage)
    {
        public static FileSplitResult Successful() => new(true, false, null);
        public static FileSplitResult Skipped(string reason) => new(false, true, reason);
        public static FileSplitResult Failed(string message) => new(false, false, message);
    }
}
