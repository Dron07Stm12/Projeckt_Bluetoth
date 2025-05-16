using Microsoft.Extensions.Logging;
#if ANDROID
using Project_Bluetooth.Platforms.Android;
#endif

namespace Project_Bluetooth
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif


            // Register the Bluetooth service
#if ANDROID
            builder.Services.AddSingleton<IBluetoothService, AndroidBluetoothService>();
#endif

            // 🔧 Регистрируем MainPage
            builder.Services.AddSingleton<MainPage>();

            return builder.Build();
        }
    }
}
