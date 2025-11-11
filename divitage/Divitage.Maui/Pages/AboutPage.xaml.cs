using Microsoft.Maui.ApplicationModel;

namespace Divitage.Maui.Pages;

public partial class AboutPage : ContentPage
{
    public AboutPage()
    {
        InitializeComponent();
    }

    private async void OnBrowserClicked(object sender, EventArgs e) => await Launcher.OpenAsync("https://c5bt.net/");
    private async void OnMailClicked(object sender, EventArgs e) => await Launcher.OpenAsync("https://c5bt.net/contact");
    private async void OnTwitterClicked(object sender, EventArgs e) => await Launcher.OpenAsync("https://twitter.com/__cho__");
    private async void OnYoutubeClicked(object sender, EventArgs e) => await Launcher.OpenAsync("https://www.youtube.com/user/akutore");
}
