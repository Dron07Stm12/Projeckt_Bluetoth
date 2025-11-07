using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Locations;
using Android.OS;
using Android.Util;
using AndroidX.Core.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Bluetooth.Platforms.Android
{

    [Service(ForegroundServiceType = ForegroundService.TypeConnectedDevice)]
   
    public class Bluetooth_Foregraund_service : Service
    {
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Проверка совместимости платформы", Justification = "<Ожидание>")]
        public override void OnCreate()
        {
            // Вызов базовой реализации.
            base.OnCreate();

            Log.Debug("BluetoothService", "OnCreate called");
            // === Создание канала уведомлений для Android 8+ ===
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O) 
            {
            
                var channel = new NotificationChannel("bluetooth_channel", "background for bluetooth devices",NotificationImportance.Max)
                {
                    Description = "data transfer from bluetooth devices"
                };

                //NotificationManager — системный сервис Android, управляющий всеми уведомлениями приложения.
#pragma warning disable CS8600 // Преобразование литерала, допускающего значение NULL или возможного значения NULL в тип, не допускающий значение NULL.
                var notificationManager = (NotificationManager)GetSystemService(NotificationService);
#pragma warning restore CS8600 // Преобразование литерала, допускающего значение NULL или возможного значения NULL в тип, не допускающий значение NULL.

                // Создание канала уведомлений.
                // pегистрирует (создаёт) канал уведомлений с параметрами, в объекте channel.
                notificationManager?.CreateNotificationChannel(channel);
                Log.Debug("BluetoothService", "Notification channel created");
            }


        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1422:Проверка совместимости платформы", Justification = "<Ожидание>")]
#pragma warning disable CS8765 // Допустимость значений NULL для типа параметра не соответствует переопределенному элементу (возможно, из-за атрибутов допустимости значений NULL).
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
#pragma warning restore CS8765 // Допустимость значений NULL для типа параметра не соответствует переопределенному элементу (возможно, из-за атрибутов допустимости значений NULL).
        {



            
            Log.Debug("BluetoothService", "OnStartCommand called");
            // Создаём уведомление для foreground-сервиса.

#pragma warning disable CS8602 // Разыменование вероятной пустой ссылки.
            var notification = new NotificationCompat.Builder(this, "bluetooth_channel")
                .SetContentTitle("Bluetooth Service")
                .SetContentText("Service is running in the foreground")
                .SetSmallIcon(Resource.Drawable.abc_btn_radio_material) // Укажите здесь свой значок уведомления.abc_btn_radio_material
                .SetOngoing(true) // Делает уведомление постоянным (неудаляемым пользователем).
                .SetPriority((int)NotificationPriority.Max) // Устанавливаем высокий приоритет для уведомления.    
                .Build();
#pragma warning restore CS8602 // Разыменование вероятной пустой ссылки.

            // Запускаем сервис в фореграунд-режиме, передавая созданное уведомление.
            StartForeground(1, notification);
            

            Log.Debug("BluetoothService", "StartForeground called");
            return StartCommandResult.Sticky;
        }






        public override IBinder? OnBind(Intent? intent)
        {
            return null;
        }
    }
}
