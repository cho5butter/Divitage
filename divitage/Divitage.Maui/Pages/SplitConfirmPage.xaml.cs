using System.IO;
using System.Threading.Tasks;
using Divitage.Maui.Models;
using Microsoft.Maui.ApplicationModel;

namespace Divitage.Maui.Pages;

public partial class SplitConfirmPage : ContentPage
{
    private readonly TaskCompletionSource<bool> _resultSource;

    private SplitConfirmPage(SplitPreview preview, TaskCompletionSource<bool> resultSource)
    {
        InitializeComponent();
        _resultSource = resultSource;
        PreviewImage.Source = ImageSource.FromStream(() => new MemoryStream(preview.PreviewPngBytes));
        FileName.Text = Path.GetFileName(preview.FilePath);
        FrameCount.Text = preview.TotalFrames.ToString();
        FileSize.Text = $"{preview.FileSizeBytes / 1024d / 1024d:F2} MB";
    }

    public static async Task<bool> RequestAsync(INavigation navigation, SplitPreview preview)
    {
        if (navigation is null) throw new ArgumentNullException(nameof(navigation));
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var page = new SplitConfirmPage(preview, tcs);
            await navigation.PushModalAsync(page);
        });

        return await tcs.Task;
    }

    private async void OnConfirm(object sender, EventArgs e)
    {
        _resultSource.TrySetResult(true);
        await Navigation.PopModalAsync();
    }

    private async void OnCancel(object sender, EventArgs e)
    {
        _resultSource.TrySetResult(false);
        await Navigation.PopModalAsync();
    }
}
