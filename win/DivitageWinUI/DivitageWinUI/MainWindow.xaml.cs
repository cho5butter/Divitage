using DivitageWinUI.Helpers;
using DivitageWinUI.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace DivitageWinUI;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        WindowHelper.Initialize(this);
    }

    private void OnNavigationLoaded(object sender, RoutedEventArgs e)
    {
        if (RootNavigation.MenuItems.Count > 0)
        {
            RootNavigation.SelectedItem = RootNavigation.MenuItems[0];
        }
    }

    private void OnNavigationSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer is not NavigationViewItem item)
        {
            return;
        }
        Navigate(item.Tag?.ToString());
    }

    private void Navigate(string? tag)
    {
        Type pageType = tag switch
        {
            "settings" => typeof(SettingsPage),
            "howto" => typeof(HowToPage),
            _ => typeof(ConverterPage)
        };

        if (ContentFrame.CurrentSourcePageType != pageType)
        {
            ContentFrame.Navigate(pageType);
        }
    }
}
