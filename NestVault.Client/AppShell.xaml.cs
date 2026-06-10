using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Font = Microsoft.Maui.Font;

namespace NestVault.Client
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            var currentTheme = Application.Current!.RequestedTheme;
            ThemeSegmentedControl.SelectedIndex = currentTheme == AppTheme.Light ? 0 : 1;
        }

        public static async Task DisplaySnackbarAsync(string message)
        {
            // The toolkit Snackbar requires packaged-app toast registration on
            // Windows, which an unpackaged app doesn't have — show an in-app
            // banner there instead.
            if (OperatingSystem.IsWindows())
            {
                await DisplayInAppBannerAsync(message);
                return;
            }

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            var snackbarOptions = new SnackbarOptions
            {
                BackgroundColor = Color.FromArgb("#2B2B33"),
                TextColor = Colors.White,
                CornerRadius = new CornerRadius(8),
                Font = Font.SystemFontOfSize(16)
            };

            var snackbar = Snackbar.Make(message, visualOptions: snackbarOptions);

            await snackbar.Show(cancellationTokenSource.Token);
        }

        private static async Task DisplayInAppBannerAsync(string message)
        {
            if (Current?.CurrentPage is not ContentPage page || page.Content is null)
                return;

            // Wrap the page content in a Grid once, so the banner can overlay it.
            if (page.Content is not Grid host || host.StyleId != "BannerHost")
            {
                var original = page.Content;
                page.Content = null;
                host = new Grid { StyleId = "BannerHost" };
                host.Children.Add(original);
                page.Content = host;
            }

            var banner = new Border
            {
                StrokeThickness = 0,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 8 },
                Background = new SolidColorBrush(Color.FromArgb("#2B2B33")),
                Padding = new Thickness(20, 12),
                Margin = new Thickness(0, 0, 0, 30),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.End,
                Opacity = 0,
                Content = new Label
                {
                    Text = message,
                    TextColor = Colors.White,
                    FontSize = 15
                }
            };

            host.Children.Add(banner);
            await banner.FadeTo(1, 150);
            await Task.Delay(2000);
            await banner.FadeTo(0, 250);
            host.Children.Remove(banner);
        }

        private void SfSegmentedControl_SelectionChanged(object? sender, Syncfusion.Maui.Toolkit.SegmentedControl.SelectionChangedEventArgs e)
        {
            Application.Current!.UserAppTheme = e.NewIndex == 0 ? AppTheme.Light : AppTheme.Dark;
        }
    }
}
