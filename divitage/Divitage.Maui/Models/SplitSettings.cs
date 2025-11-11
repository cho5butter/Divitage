namespace Divitage.Maui.Models;

public enum SaveLocationOption
{
    SameDirectory = 0,
    CustomDirectory = 1
}

public enum ImageFormatOption
{
    Jpeg = 0,
    Bmp = 1,
    Tiff = 2,
    Png = 3,
    Gif = 4
}

public enum NameConventionOption
{
    Original = 0,
    DateOriginal = 1,
    TimeOriginal = 2,
    DateTimeOriginal = 3,
    RandomOriginal = 4
}

public enum SplitMode
{
    FrameInterval = 0,
    Percentage = 1
}

public sealed record SplitSettings(
    SaveLocationOption SaveLocation,
    string? CustomDirectory,
    bool ConfirmBeforeSplit,
    ImageFormatOption ImageFormat,
    NameConventionOption NameConvention,
    SplitMode Mode,
    int SplitInterval,
    bool RestrictFrameRange,
    int StartFrame,
    int EndFrame);

public sealed record SplitPreview(
    string FilePath,
    int TotalFrames,
    long FileSizeBytes,
    int PreviewFrameIndex,
    byte[] PreviewPngBytes);

public sealed record SplitProgress(double Percent, string Message);

public sealed record SplitSummary(int RequestedFiles, int CompletedFiles, int SkippedFiles, IReadOnlyList<string> Errors);
