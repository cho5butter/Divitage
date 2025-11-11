using Microsoft.Maui.Controls;

namespace Divitage.Maui.Helpers;

public static class ServiceHelper
{
    public static T GetService<T>() where T : notnull
    {
        if (Application.Current?.Handler?.MauiContext?.Services is IServiceProvider services)
        {
            var service = services.GetService(typeof(T));
            if (service is T typed)
            {
                return typed;
            }
        }

        throw new InvalidOperationException($"サービス {typeof(T)} が解決できませんでした。");
    }
}
