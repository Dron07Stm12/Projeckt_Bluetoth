using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;

namespace Project_Bluetooth
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {

        public static MainActivity Instance { get; private set; }


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);


        }


        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            // Прокидываем результат в сервис (example singleton)
            Project_Bluetooth.Platforms.Android.AndroidBluetoothService.Instance?.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }


    }
}




//🔸 Что происходит:
//Build.VERSION.SdkInt — текущая версия Android API устройства.
//Android.OS.BuildVersionCodes.R — это API 30, т.е.Android 11.
//Условие: если Android > 11(то есть Android 12 и выше)
//🔹 Общая цель:
//Эта строка(ActivityCompat.CheckSelfPermission(this, Manifest.Permission.BluetoothConnect) != Permission.Granted) проверяет,
//выдал ли пользователь приложению разрешение на подключение к Bluetooth - устройствам на Android 12 +.
//this,                               // текущая Activity (контекст выполнения)
//Manifest.Permission.BluetoothConnect // нужное разрешение
//Это строковая константа, представляющая имя разрешения, которое нужно приложению.
//Она равна "android.permission.BLUETOOTH_CONNECT" — то есть новое разрешение, введённое с Android 12(API 31).
//!= Permission.Granted
//Permission.Granted — это просто 0(код, означающий "разрешение предоставлено").
//Если CheckSelfPermission(...) возвращает не Permission.Granted, значит: 👉 разрешение ещё не предоставлено,
//и его нужно запросить через RequestPermissions(...).

//и если разрешение BluetoothConnect не предоставлено, тогда:

//if (Build.VERSION.SdkInt > Android.OS.BuildVersionCodes.R && ActivityCompat.CheckSelfPermission(this, Manifest.Permission.BluetoothConnect) != Permission.Granted)
//{


//    //🔸 Запрашиваем разрешение на BluetoothConnect
//    //🔸 Что происходит:
//    //Вызывается метод RequestPermissions, чтобы запросить разрешение BluetoothConnect.
//    //102 — это произвольный код запроса(можешь использовать его позже в OnRequestPermissionsResult).
//    ActivityCompat.RequestPermissions(Microsoft.Maui.ApplicationModel.Platform.CurrentActivity, new string[] { Android.Manifest.Permission.BluetoothConnect }, 102);
//}
//if (Build.VERSION.SdkInt <= Android.OS.BuildVersionCodes.R && ActivityCompat.CheckSelfPermission(this, Manifest.Permission.Bluetooth) != Permission.Granted)
//{
//    ActivityCompat.RequestPermissions(Microsoft.Maui.ApplicationModel.Platform.CurrentActivity, new string[] { Android.Manifest.Permission.Bluetooth }, 102);

//}

////🔹 Дополнительная проверка для Android 12+ (SDK >= S, т.е. API 31)
//if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.S)
//{

//    //🔸 Запрашиваем разрешение на BluetoothScan и BluetoothConnect

//    if (ContextCompat.CheckSelfPermission(Platform.CurrentActivity, Manifest.Permission.BluetoothScan) != Android.Content.PM.Permission.Granted ||
//        ContextCompat.CheckSelfPermission(Platform.CurrentActivity, Manifest.Permission.BluetoothConnect) != Android.Content.PM.Permission.Granted)
//    {
//        //Запрашиваются оба разрешения одновременно.
//        ActivityCompat.RequestPermissions(Platform.CurrentActivity, new string[]
//        {
//    Manifest.Permission.BluetoothScan,
//    Manifest.Permission.BluetoothConnect
//        }, 1);
//    }
//}

