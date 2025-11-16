using DivitageWinUI.Helpers;
using Microsoft.UI.Xaml;

namespace DivitageWinUI;

public partial class App : Application
{
    public static MainWindow? MainWindow { get; private set; }

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new MainWindow();

        // 保存されているテーマを適用
        ApplySavedTheme();

        MainWindow.Activate();
    }

    private void ApplySavedTheme()
    {
        if (MainWindow?.Content is FrameworkElement rootElement)
        {
            var theme = SettingsHelper.Theme;
            rootElement.RequestedTheme = theme switch
            {
                1 => ElementTheme.Light,
                2 => ElementTheme.Dark,
                _ => ElementTheme.Default
            };
        }
    }
}
