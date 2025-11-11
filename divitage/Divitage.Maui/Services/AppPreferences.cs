using Divitage.Maui.Models;
using Microsoft.Maui.Storage;

namespace Divitage.Maui.Services;

public sealed class AppPreferences
{
    private const string SaveLocationKey = "save_location";
    private const string CustomDirectoryKey = "custom_directory";
    private const string ConfirmKey = "confirm_before";
    private const string ImageFormatKey = "image_format";
    private const string NameConventionKey = "name_convention";
    private const string SplitModeKey = "split_mode";
    private const string SplitIntervalKey = "split_interval";
    private const string FrameRangeKey = "frame_range";
    private const string StartFrameKey = "start_frame";
    private const string EndFrameKey = "end_frame";

    public SaveLocationOption SaveLocation
    {
        get => (SaveLocationOption)Preferences.Get(SaveLocationKey, (int)SaveLocationOption.SameDirectory);
        set => Preferences.Set(SaveLocationKey, (int)value);
    }

    public string? CustomDirectory
    {
        get => Preferences.Get(CustomDirectoryKey, string.Empty);
        set => Preferences.Set(CustomDirectoryKey, value ?? string.Empty);
    }

    public bool ConfirmBeforeSplit
    {
        get => Preferences.Get(ConfirmKey, false);
        set => Preferences.Set(ConfirmKey, value);
    }

    public ImageFormatOption ImageFormat
    {
        get => (ImageFormatOption)Preferences.Get(ImageFormatKey, (int)ImageFormatOption.Jpeg);
        set => Preferences.Set(ImageFormatKey, (int)value);
    }

    public NameConventionOption NameConvention
    {
        get => (NameConventionOption)Preferences.Get(NameConventionKey, (int)NameConventionOption.Original);
        set => Preferences.Set(NameConventionKey, (int)value);
    }

    public SplitMode Mode
    {
        get => (SplitMode)Preferences.Get(SplitModeKey, (int)SplitMode.FrameInterval);
        set => Preferences.Set(SplitModeKey, (int)value);
    }

    public int SplitInterval
    {
        get => Math.Max(1, Preferences.Get(SplitIntervalKey, 1));
        set => Preferences.Set(SplitIntervalKey, Math.Max(1, value));
    }

    public bool RestrictFrameRange
    {
        get => Preferences.Get(FrameRangeKey, false);
        set => Preferences.Set(FrameRangeKey, value);
    }

    public int StartFrame
    {
        get => Preferences.Get(StartFrameKey, -1);
        set => Preferences.Set(StartFrameKey, value);
    }

    public int EndFrame
    {
        get => Preferences.Get(EndFrameKey, -1);
        set => Preferences.Set(EndFrameKey, value);
    }

    public SplitSettings ToSettings() => new(
        SaveLocation,
        CustomDirectory,
        ConfirmBeforeSplit,
        ImageFormat,
        NameConvention,
        Mode,
        SplitInterval,
        RestrictFrameRange,
        StartFrame,
        EndFrame);
}
