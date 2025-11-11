using Divitage.Maui.Pages;
using Divitage.Maui.Services;

namespace Divitage.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>();

        builder.Services.AddSingleton<AppPreferences>();
        builder.Services.AddSingleton<VideoSplitService>();

        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<AboutPage>();

        return builder.Build();
    }
}
