using System;
using System.Collections.ObjectModel;
using Project_Bluetooth.Models;
using DeviceInfo = Project_Bluetooth.Models.DeviceInfo;
using Microsoft.Maui.Devices;




#if ANDROID
using AndroidBluetoothService = Project_Bluetooth.Platforms.Android.AndroidBluetoothService;
#endif


namespace Project_Bluetooth
{
    public partial class MainPage : ContentPage
    {
       
        // Bluetooth service
        private readonly IBluetoothService _bluetoothService;
        // Коллекция найденных устройств
        public ObservableCollection<DeviceInfo> DiscoveredDevices { get; } = new();


        public MainPage(IBluetoothService bluetoothService)
        {
            InitializeComponent();
            _bluetoothService = bluetoothService;

            BindingContext = this;

#if ANDROID
            // Передаём делегат в Bluetooth-сервис
            //Это проверка типа: мы проверяем, является ли внедрённый сервис _bluetoothService
            //конкретной реализацией AndroidBluetoothService.
            // Если это так, то приводим его к этому типу и сохраняем в переменную androidService.
       
            if (_bluetoothService is AndroidBluetoothService androidService2)
            {
                //подписываемся на событие DeviceDiscovered - добавляем обработчик события
                androidService2.DeviceDiscovered += delegate (DeviceInfo info)
                {
                    // Главный поток для UI, чтоб в UI было изменение коллекции DiscoveredDevices
                    MainThread.BeginInvokeOnMainThread(delegate ()
                    {
                        //проверяем, есть ли уже устройство в коллекции DiscoveredDevices
                        Func<DeviceInfo, bool> predicate = delegate (DeviceInfo deviceInfo)
                        {
                            return deviceInfo.Address == info.Address;
                        };
                        //Если устройства нет в коллекции DiscoveredDevices, то добавляем его
                        if (!DiscoveredDevices.Any(predicate)) { DiscoveredDevices.Add(info); }

                    });

                };
            }
#endif

        }


        private async void On_Off_Bluetooth(object sender, EventArgs e)
        {

            await _bluetoothService.OnOffBluetoothAsyncc();
            //await _bluetoothService.OnOffBluetoothAsyncc();
        }

        private async void OnStartScanClicked(object sender, EventArgs e)
        {
           
            await _bluetoothService.StartScanningAsync();
        }

      

        private async void OnStopScanClicked(object sender, EventArgs e)
        {
            await _bluetoothService.StopScanningAsync();
            // await _bluetoothService.StopScanningAsync();
        }
        // OnScanClicked


    }

}


//Action<DeviceInfo> action = delegate (DeviceInfo info)
//{
//    MainThread.BeginInvokeOnMainThread(() =>
//    {
//        if (!DiscoveredDevices.Any(d => d.Address == info.Address))
//        {
//            DiscoveredDevices.Add(info);
//        }
//    });
//};



//if (_bluetoothService is AndroidBluetoothService androidService2)
//{

//    Action<DeviceInfo> action = delegate (DeviceInfo info)
//    {

//        Func<DeviceInfo, bool> predicate = delegate (DeviceInfo deviceInfo)
//        {
//            return deviceInfo.Address == info.Address;
//        };


//        MainThread.BeginInvokeOnMainThread(delegate ()
//        {
//            if (!DiscoveredDevices.Any(predicate))
//            {
//                DiscoveredDevices.Add(info);
//            }
//        });
//    };

//    //Подписываемся на событие DeviceDiscovered
//    androidService2.DeviceDiscovered += action;


//}



//if (_bluetoothService is AndroidBluetoothService androidService)
//{
//    androidService.DeviceDiscovered += device =>
//    {
//        MainThread.BeginInvokeOnMainThread(() =>
//        {
//            if (!DiscoveredDevices.Any(d => d.Address == device.Address))
//            {
//                DiscoveredDevices.Add(device);
//            }
//        });
//    };
//}


