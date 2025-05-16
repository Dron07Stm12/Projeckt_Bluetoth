
using Android;
using Android.Bluetooth;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Project_Bluetooth.Models;
using static Microsoft.Maui.ApplicationModel.Permissions;
using DeviceInfo = Project_Bluetooth.Models.DeviceInfo;
using Android.Provider;

namespace Project_Bluetooth.Platforms.Android
{
    public class AndroidBluetoothService : IBluetoothService
    {
        private BluetoothAdapter _adapter;
        private BroadcastReceiver _receiver;
        private Context _context;
        private BluetoothDevice _connectedDevice;
        private Intent? intent2;

        // ✅ Добавляем событие для передачи найденных устройств
        public event Action<DeviceInfo> DeviceDiscovered;

        public AndroidBluetoothService()
        {
            _adapter = BluetoothAdapter.DefaultAdapter;
            _context = Platform.AppContext; // ✅ Используем MAUI Platform.AppContext вместо Android.App.Application.Context
           
        }

        //public async Task StartScanningAsync()
        //{
        //    if (!_adapter.IsEnabled)
        //    {
        //        await Application.Current.MainPage.DisplayAlert("Bluetooth", "Bluetooth отключён", "OK");
        //        return;
        //    }

        //    if (_receiver != null)
        //    {
        //        _context.UnregisterReceiver(_receiver);
        //        _receiver = null;
        //    }


        //    //_receiver = new DeviceReceiver(device =>
        //    //{
        //    //    DeviceDiscovered?.Invoke(device);
        //    //});

        //    _receiver = new DeviceReceiver(device =>
        //    {
        //        MainThread.BeginInvokeOnMainThread(() =>
        //        {
        //            Application.Current.MainPage.DisplayAlert("Найдено", $"{device.Name} ({device.Address})", "OK");
        //        });
        //    });

        //    IntentFilter filter = new IntentFilter(BluetoothDevice.ActionFound);
        //    _context.RegisterReceiver(_receiver, filter);

        //    _adapter.StartDiscovery();
        //}

        //public async Task StartScanningAsync()
        //{
        //    // ✅ Проверка и запрос разрешений для Android 12+
        //                var permissions = new[]
        //                {

        //                    Manifest.Permission.BluetoothScan,
        //                    Manifest.Permission.BluetoothConnect,
        //                    Manifest.Permission.AccessFineLocation
        //                };

        //    foreach (var permission in permissions)
        //    {
        //        if (ContextCompat.CheckSelfPermission(Platform.CurrentActivity, permission) != Permission.Granted)
        //        {
        //            ActivityCompat.RequestPermissions(Platform.CurrentActivity, permissions, 0);
        //            await Application.Current.MainPage.DisplayAlert("Разрешения", "Bluetooth-разрешения требуются", "OK");
        //            return;
        //        }
        //    }

        //    if (!_adapter.IsEnabled)
        //    {
        //        await Application.Current.MainPage.DisplayAlert("Bluetooth", "Bluetooth отключён", "OK");
        //        return;
        //    }

        //    if (_receiver != null)
        //    {
        //        _context.UnregisterReceiver(_receiver);
        //        _receiver = null;
        //    }

        //    _receiver = new DeviceReceiver(device =>
        //    {
        //        DeviceDiscovered?.Invoke(device);
        //    });

        //    IntentFilter filter = new IntentFilter(BluetoothDevice.ActionFound);
        //    _context.RegisterReceiver(_receiver, filter);

        //    _adapter.StartDiscovery();
        //}

        public async Task StartScanningAsync() {

            Permission permission_BluetoothScan = AndroidX.Core.Content.ContextCompat.CheckSelfPermission(_context, Manifest.Permission.BluetoothScan);
            Permission permission_BluetoothConnect = AndroidX.Core.Content.ContextCompat.CheckSelfPermission(_context, Manifest.Permission.BluetoothConnect);
            Permission permission_AccessFineLocation = AndroidX.Core.Content.ContextCompat.CheckSelfPermission(_context, Manifest.Permission.AccessFineLocation);
            string [] permission_BluetoothScanString  = { Manifest.Permission.BluetoothScan,Manifest.Permission.BluetoothConnect,Manifest.Permission.AccessFineLocation };
           
            if (Build.VERSION.SdkInt >= BuildVersionCodes.S) 
            {
                if ( permission_BluetoothScan != Permission.Granted)
                {  ActivityCompat.RequestPermissions(Platform.CurrentActivity, new [] { Manifest.Permission.BluetoothScan }, 0);
                    await Application.Current.MainPage.DisplayAlert("Разрешения", "Bluetooth-разрешения требуются (Android 12+)", "OK");
                }
                if (permission_BluetoothConnect != Permission.Granted)
                { ActivityCompat.RequestPermissions(Platform.CurrentActivity, new[] { Manifest.Permission.BluetoothConnect }, 0);
                   await Application.Current.MainPage.DisplayAlert("Разрешения", "Bluetooth-разрешения требуются (Android 12+)", "OK");
                }
               
                                                      
            }
            else {

                if (permission_AccessFineLocation != Permission.Granted)
                {
                    ActivityCompat.RequestPermissions(Platform.CurrentActivity, new[] { Manifest.Permission.AccessFineLocation }, 0);
                    await Application.Current.MainPage.DisplayAlert("Разрешения", "Bluetooth-разрешения требуются (Android < 12)", "OK");
                }


            }


           


            if (!_adapter.IsEnabled)
            {
                await Application.Current.MainPage.DisplayAlert("Bluetooth", "Bluetooth отключён", "OK");
                return;
            }

            await Application.Current.MainPage.DisplayAlert("Разрешения", "Bluetooth-разрешения запрошены", "OK");

            if (_receiver != null)
            {
                //🔹 Очистка предыдущего приёмника(если был)
                // Если приёмник уже существует, то мы его удаляем
                _context.UnregisterReceiver(_receiver);
                _receiver = null;
            }
            //🔹 Создание нового приёмника
            //_receiver = new DeviceReceiver(device =>
            //{
            //    DeviceDiscovered?.Invoke(device);
            //});

            // Что здесь происходит:
            //Action < DeviceInfo > — это делегат, который принимает один параметр типа DeviceInfo и ничего не возвращает(void).
            //Вы создаёте анонимный метод(анонимный делегат), который будет вызван, когда будет обнаружено новое Bluetooth - устройство.
            //Внутри этого делегата вызывается событие DeviceDiscovered, если на него кто - то подписан(!= null).
            // а на него подписан метод, который находиться в MainPage.xaml.cs

            // Создаём делегат, который будет вызываться при нахождении устройства
            // Внутри этого делегата вызывается событие DeviceDiscovered,
            // если на него кто - то подписан(!= null).

            Action<DeviceInfo> action = delegate (DeviceInfo device)
            {
                // Внутри этого делегата вызывается событие DeviceDiscovered,
                // если на него кто - то подписан(!= null).
                //"Происходит вызов события DeviceDiscovered, и все подписанные на него обработчики (в данном случае — из MainPage)
                //будут вызваны."
            if (DeviceDiscovered != null) { DeviceDiscovered(device);}
            };
            // Создаём экземпляр DeviceReceiver и передаём ему делегат
            _receiver = new DeviceReceiver(action);
            //🔹Фильтрация  приёмника на событие ACTION_FOUND
            IntentFilter filter = new IntentFilter(BluetoothDevice.ActionFound);
            //Регистрируем BroadcastReceiver, чтобы получать уведомления, когда найдено Bluetooth-устройство.
            _context.RegisterReceiver(_receiver, filter);

            _adapter.StartDiscovery();



        }
        //public async Task StartScanningAsync()
        //{
        //    // Android 12 и выше (API 31+)
        //    if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
        //    {
        //        var permissions = new[]
        //        {
        //            Manifest.Permission.BluetoothScan,    // — разрешение на поиск Bluetooth-устройств, появилось в Android 12.
        //            Manifest.Permission.BluetoothConnect  // - — разрешение на подключение к Bluetooth-устройствам, появилось в Android 12.
        //        };

        //        foreach (var permission in permissions)
        //        {
        //            if (ContextCompat.CheckSelfPermission(Platform.CurrentActivity, permission) != Permission.Granted)
        //            {
        //                ActivityCompat.RequestPermissions(Platform.CurrentActivity, permissions, 0);
        //                await Application.Current.MainPage.DisplayAlert("Разрешения", "Bluetooth-разрешения требуются (Android 12+)", "OK");
        //                return;
        //            }
                
        //        }
        //    }
        //    else // Android 6–11 (API 23–30)
        //    {
        //        var legacyPermission = Manifest.Permission.AccessFineLocation;

        //        if (ContextCompat.CheckSelfPermission(Platform.CurrentActivity, legacyPermission) != Permission.Granted)
        //        {
        //            ActivityCompat.RequestPermissions(Platform.CurrentActivity, new[] { legacyPermission }, 0);
        //            await Application.Current.MainPage.DisplayAlert("Разрешения", "Разрешение на геолокацию требуется (Android < 12)", "OK");
        //            return;
        //        }
        //    }

        //    if (!_adapter.IsEnabled)
        //    {
        //        await Application.Current.MainPage.DisplayAlert("Bluetooth", "Bluetooth отключён", "OK");
        //        return;
        //    }

        //    if (_receiver != null)
        //    {
        //        _context.UnregisterReceiver(_receiver);
        //        _receiver = null;
        //    }

        //    _receiver = new DeviceReceiver(device =>
        //    {
        //        DeviceDiscovered?.Invoke(device);
        //    });

        //    IntentFilter filter = new IntentFilter(BluetoothDevice.ActionFound);
        //    _context.RegisterReceiver(_receiver, filter);

        //    _adapter.StartDiscovery();
        //}


        public Task StopScanningAsync()
        {
            if (_adapter.IsDiscovering)
                _adapter.CancelDiscovery();

            if (_receiver != null)
            {
                _context.UnregisterReceiver(_receiver);
                _receiver = null;
            }

            return Task.CompletedTask;
        }

        public async Task ConnectToDeviceAsync(string address)
        {
            var device = _adapter?.BondedDevices?.FirstOrDefault(d => d.Address == address);
            if (device == null)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Устройство не найдено", "OK");
                return;
            }

            _connectedDevice = device;
            await Application.Current.MainPage.DisplayAlert("Успех", $"Подключено к {_connectedDevice.Name}", "OK");
        }

        public async Task DisconnectFromDeviceAsync()
        {
            if (_connectedDevice != null)
            {
                await Application.Current.MainPage.DisplayAlert("Отключено", $"Отключено от {_connectedDevice.Name}", "OK");
                _connectedDevice = null;
            }
        }





        public async Task OnOffBluetoothAsyncc()
        {


            try
            {
                // 🔍 Проверяем, инициализирован ли адаптер Bluetooth
                //🔹 Проверка на инициализацию Bluetooth адаптера.Если _adapter == null, значит устройство не поддерживает Bluetooth
                //    или не удалось получить доступ к нему. Выводится сообщение и выход из метода.
                if (_adapter == null)
                {
                    await Application.Current.MainPage.DisplayAlert("Ошибка", "Bluetooth-адаптер не найден", "OK");
                    return;
                }
                //🔹 Проверка версии Android. Если это Android 13 (Tiramisu) или новее, действуют новые ограничения
                //— нельзя программно включать/выключать Bluetooth, только через настройки.

                bool isModernAndroid = Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu;

                if (_adapter.IsEnabled)//Bluetooth включён?
                {
                    if (isModernAndroid)
                    {
                        // Android 13+: нельзя программно отключить Bluetooth, открываем настройки
                        //Intent Класс Android для запуска действий(в данном случае — открыть настройки).
                        
                        //Settings.ActionBluetoothSettings	Константа: "android.settings.BLUETOOTH_SETTINGS".
                        //Открывает системные Bluetooth-настройки.

                        var intent = new Intent(Settings.ActionBluetoothSettings);

                        //AddFlags(ActivityFlags.NewTask) Говорит Android, что Intent нужно запускать
                        //в новой задаче. Это обязательно при запуске из Context, а не из Activity.
                        intent.AddFlags(ActivityFlags.NewTask);
                        _context.StartActivity(intent);
                        ///////////////////////


                    }
                    else
                    {
                        // Android < 13: отключаем Bluetooth через рефлексию
                        try
                        {
                            var disableMethod = _adapter.Class.GetMethod("disable");
                            disableMethod?.Invoke(_adapter);
                            await Application.Current.MainPage.DisplayAlert("Bluetooth", "Bluetooth отключён", "OK");
                        }
                        catch (Exception ex)
                        {
                            await Application.Current.MainPage.DisplayAlert("Ошибка", $"Не удалось отключить Bluetooth: {ex.Message}", "OK");
                        }
                    }
                }
                else
                {
                    if (isModernAndroid)
                    {
                        // Android 13+: перенаправление в настройки
                        var intent = new Intent("android.settings.BLUETOOTH_SETTINGS");
                        intent.AddFlags(ActivityFlags.NewTask);
                        _context.StartActivity(intent);
                    }
                    else
                    {
                        bool enabled = _adapter.Enable();
                        string message = enabled ? "Bluetooth включён" : "Не удалось включить Bluetooth";
                        await Application.Current.MainPage.DisplayAlert("Bluetooth", message, "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", $"Bluetooth ошибка: {ex.Message}", "OK");
            }

        }
    }
}




//💡 Подытожим коротко, для закрепления:
//Этап Что происходит
//1.	AndroidBluetoothService вызывает RegisterReceiver() с DeviceReceiver и фильтром BluetoothDevice.ActionFound.
//2.	Запускается сканирование через _adapter.StartDiscovery().
//3.	Когда найдено устройство, Android вызывает DeviceReceiver.OnReceive().
//4.	В OnReceive формируется DeviceInfo, вызывается делегат _onDeviceFound.
//5.	Этот делегат вызывает событие DeviceDiscovered, если на него подписались.
//6.	В MainPage, где подписались на DeviceDiscovered, вызывается обработчик.
//7.	Через MainThread.BeginInvokeOnMainThread добавляем новое устройство в DiscoveredDevices.
