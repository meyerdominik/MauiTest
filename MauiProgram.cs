using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace MauiTest
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit(options =>
                {
                    var popupSettings = new DefaultPopupSettings()
                    {
                        // BackgroundColor // there is no way to bind this to the AppTheme
                    };

                    // options.SetPopupDefaults(popupSettings);
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddTransientPopup<PopupContentView>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
