using System;
using System.IO;
using Windows.Storage;

namespace DivitageWinUI.Helpers;

/// <summary>
/// アプリケーション設定を管理するヘルパークラス
/// </summary>
public static class SettingsHelper
{
    private static readonly ApplicationDataContainer _settings = ApplicationData.Current.LocalSettings;

    /// <summary>
    /// 出力ディレクトリが変更されたときに発生するイベント
    /// </summary>
    public static event EventHandler<string>? OutputDirectoryChanged;

    /// <summary>
    /// 出力ディレクトリを取得または設定します
    /// </summary>
    public static string OutputDirectory
    {
        get
        {
            if (_settings.Values["outputDirectory"] is string savedPath && !string.IsNullOrWhiteSpace(savedPath))
            {
                return savedPath;
            }

            var downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            var defaultPath = Path.Combine(downloads, "DivitageOutput");
            OutputDirectory = defaultPath; // 初回は保存する
            return defaultPath;
        }
        set
        {
            if (value != OutputDirectory)
            {
                _settings.Values["outputDirectory"] = value;
                try
                {
                    Directory.CreateDirectory(value);
                }
                catch
                {
                    // ディレクトリの作成に失敗しても続行
                }
                OutputDirectoryChanged?.Invoke(null, value);
            }
        }
    }

    /// <summary>
    /// ログイン時に起動するかどうかを取得または設定します
    /// </summary>
    public static bool LaunchAtLogin
    {
        get => _settings.Values["launchAtLogin"] is bool value && value;
        set => _settings.Values["launchAtLogin"] = value;
    }

    /// <summary>
    /// 処理完了後に一時ファイルを削除するかどうかを取得または設定します
    /// </summary>
    public static bool AutoCleanup
    {
        get => _settings.Values["autoCleanup"] is bool value ? value : true;
        set => _settings.Values["autoCleanup"] = value;
    }

    /// <summary>
    /// タイムスタンプを保持するかどうかを取得または設定します
    /// </summary>
    public static bool PreserveTimestamp
    {
        get => _settings.Values["preserveTimestamp"] is bool value ? value : true;
        set => _settings.Values["preserveTimestamp"] = value;
    }

    /// <summary>
    /// アプリケーションテーマを取得または設定します（0: System, 1: Light, 2: Dark）
    /// </summary>
    public static int Theme
    {
        get => _settings.Values["theme"] is int value ? value : 0;
        set => _settings.Values["theme"] = value;
    }

    /// <summary>
    /// 選択されているプロファイルIDを取得または設定します
    /// </summary>
    public static string? SelectedProfileId
    {
        get => _settings.Values["selectedProfileId"] as string;
        set => _settings.Values["selectedProfileId"] = value;
    }

    /// <summary>
    /// 変換履歴を保持する日数を取得または設定します
    /// </summary>
    public static int HistoryRetentionDays
    {
        get => _settings.Values["historyRetentionDays"] is int value ? value : 90;
        set => _settings.Values["historyRetentionDays"] = value;
    }
}
