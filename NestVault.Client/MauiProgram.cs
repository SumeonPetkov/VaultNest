using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Hosting;

namespace NestVault.Client
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureSyncfusionToolkit()
                .ConfigureMauiHandlers(handlers =>
                {
#if WINDOWS
    				Microsoft.Maui.Controls.Handlers.Items.CollectionViewHandler.Mapper.AppendToMapping("KeyboardAccessibleCollectionView", (handler, view) =>
    				{
    					handler.PlatformView.SingleSelectionFollowsFocus = false;
    				});
#endif
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
                    fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
                });

#if DEBUG
    		builder.Logging.AddDebug();
    		builder.Services.AddLogging(configure => configure.AddDebug());
#endif

            builder.Services.AddSingleton(_ =>
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri(Constants.ApiBaseUrl)
                };
                // The API records the User-Agent on login sessions and requires it.
                client.DefaultRequestHeaders.UserAgent.ParseAdd($"NestVaultClient/{AppInfo.Current.VersionString} ({DeviceInfo.Platform})");
                return client;
            });

            builder.Services.AddSingleton<TokenStore>();
            builder.Services.AddSingleton<AuthApiService>();
            builder.Services.AddSingleton<VaultCryptoService>();
            builder.Services.AddSingleton<PasswordGeneratorService>();
            builder.Services.AddSingleton<VaultItemRepository>();
            builder.Services.AddSingleton<ModalErrorHandler>();

            builder.Services.AddSingleton<LoginPageModel>();
            builder.Services.AddSingleton<RegisterPageModel>();
            builder.Services.AddSingleton<VaultPageModel>();
            builder.Services.AddSingleton<GeneratorPageModel>();
            builder.Services.AddSingleton<SettingsPageModel>();

            builder.Services.AddTransientWithShellRoute<VaultItemDetailPage, VaultItemDetailPageModel>("item");

            return builder.Build();
        }
    }
}
