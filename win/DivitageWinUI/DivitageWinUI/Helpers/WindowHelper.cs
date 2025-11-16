using Microsoft.UI.Xaml;
using System;
using WinRT.Interop;

namespace DivitageWinUI.Helpers;

/// <summary>
/// ウィンドウハンドルを管理するヘルパークラス
/// </summary>
internal static class WindowHelper
{
    private static Window? _window;

    /// <summary>
    /// ウィンドウを初期化します
    /// </summary>
    /// <param name="window">初期化するウィンドウ</param>
    public static void Initialize(Window window)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));
    }

    /// <summary>
    /// ウィンドウハンドルを取得します
    /// </summary>
    public static IntPtr Handle
    {
        get
        {
            if (_window is null)
            {
                throw new InvalidOperationException("ウィンドウが初期化されていません。WindowHelper.Initialize() を先に呼び出してください。");
            }
            return WindowNative.GetWindowHandle(_window);
        }
    }
}
