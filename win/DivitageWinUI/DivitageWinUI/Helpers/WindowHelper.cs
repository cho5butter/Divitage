using Microsoft.UI.Xaml;
using System;
using WinRT.Interop;

namespace DivitageWinUI.Helpers;

internal static class WindowHelper
{
    private static Window? _window;

    public static void Initialize(Window window)
    {
        _window = window;
    }

    public static IntPtr Handle =>
        _window is null ? IntPtr.Zero : WindowNative.GetWindowHandle(_window);
}
