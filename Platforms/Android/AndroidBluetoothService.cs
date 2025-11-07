
using Android;
using Android.Bluetooth;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Project_Bluetooth.Models;
using System.Text;
using System.Threading.Tasks;
using static Android.Icu.Text.IDNA;
using static Microsoft.Maui.ApplicationModel.Permissions;
using DeviceInfo = Project_Bluetooth.Models.DeviceInfo;


namespace Project_Bluetooth.Platforms.Android
{
    public class AndroidBluetoothService : IBluetoothService
    {      
        private BluetoothAdapter _adapter;
        private BroadcastReceiver _receiver;
        private Context _context;
        private BluetoothDevice _connectedDevice;
        public BluetoothSocket? socket_global; // Переменная для хранения глобального сокета подключения к выбранному устройству
        public int i = 0; // Переменная для хранения глобального сокета подключения к выбранному устройству
        public byte[] bytes = new byte[4096]; // Переменная для хранения глобального сокета подключения к выбранному устройству
        public static AndroidBluetoothService Instance { get; private set; }

        private TaskCompletionSource<bool>? _permissionTcs;



        // ✅ Добавляем событие для передачи найденных устройств
        public event Action<DeviceInfo> DeviceDiscovered;

        public AndroidBluetoothService()
        {
            Instance = this; // <-- Эта строка сделает ваш сервис singleton'ом!
            _adapter = BluetoothAdapter.DefaultAdapter;
            _context = Platform.AppContext; // ✅ Используем MAUI Platform.AppContext вместо Android.App.Application.Context
           
        }



        public Task<bool> RequestBluetoothPermissionsAsync()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
            {
                if (ContextCompat.CheckSelfPermission(Platform.CurrentActivity, Manifest.Permission.BluetoothScan) != Permission.Granted ||
                    ContextCompat.CheckSelfPermission(Platform.CurrentActivity, Manifest.Permission.BluetoothConnect) != Permission.Granted ||
                    ContextCompat.CheckSelfPermission(Platform.CurrentActivity, Manifest.Permission.AccessFineLocation) != Permission.Granted)
                {
                    _permissionTcs = new TaskCompletionSource<bool>();
                    ActivityCompat.RequestPermissions(
                        Platform.CurrentActivity,
                        new[] {
                    Manifest.Permission.BluetoothScan,
                    Manifest.Permission.BluetoothConnect,
                    Manifest.Permission.AccessFineLocation // <-- добавь это!
                        },
                        101);
                    return _permissionTcs.Task;
                }
                return Task.FromResult(true);
            }
            else
            {
                if (ContextCompat.CheckSelfPermission(Platform.CurrentActivity, Manifest.Permission.Bluetooth) != Permission.Granted ||
                    ContextCompat.CheckSelfPermission(Platform.CurrentActivity, Manifest.Permission.AccessFineLocation) != Permission.Granted)
                {
                    _permissionTcs = new TaskCompletionSource<bool>();
                    ActivityCompat.RequestPermissions(
                        Platform.CurrentActivity,
                        new[] {
                    Manifest.Permission.Bluetooth,
                    Manifest.Permission.AccessFineLocation // <-- и тут!
                        },
                        102);
                    return _permissionTcs.Task;
                }
                return Task.FromResult(true);
            }
        }
        public void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            if (_permissionTcs == null) return;
            if (requestCode == 101 || requestCode == 102)
            {
                bool granted = grantResults.All(r => r == Permission.Granted);
                _permissionTcs.TrySetResult(granted);
                _permissionTcs = null;
            }
        }





        // Приватное поле для хранения подписчиков
        //private MyEventHandler _myEvent;
        //// Явная реализация события интерфейса IBluetoothService
        //event MyEventHandler IBluetoothService.MyEvent
        //{
        //    add { _myEvent += value; }  // Добавить обработчик
        //    remove { _myEvent -= value; }  // Удалить обработчик
        //}



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


        //public static bool RequestBluetoothPermissions()
        //{
        //    if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
        //    {
        //        // Android 12+ (API 31+)
        //        if (ContextCompat.CheckSelfPermission(Platform.CurrentActivity, Manifest.Permission.BluetoothScan) != Permission.Granted ||
        //            ContextCompat.CheckSelfPermission(Platform.CurrentActivity, Manifest.Permission.BluetoothConnect) != Permission.Granted)
        //        {
        //            ActivityCompat.RequestPermissions(
        //                Platform.CurrentActivity,
        //                new[] { Manifest.Permission.BluetoothScan, Manifest.Permission.BluetoothConnect },
        //                0);
        //            return false;
        //        }
        //        return true;
        //    }
        //    else
        //    {
        //        // Android < 12
        //        if (ContextCompat.CheckSelfPermission(Platform.CurrentActivity, Manifest.Permission.Bluetooth) != Permission.Granted)
        //        {
        //            ActivityCompat.RequestPermissions(
        //                Platform.CurrentActivity,
        //                new[] { Manifest.Permission.Bluetooth },
        //                0);
        //            return false;
        //        }
        //        return true;
        //    }
        //}





        private async Task<bool> CheckPermissions()
        {
#if ANDROID
            // Запрашиваем разрешение на использование геолокации (требуется для поиска Bluetooth-устройств в Android)
            var statusLoc = await RequestAsync<LocationWhenInUse>();
         
            // Запрашиваем разрешение на использование Bluetooth
            var statusConnect = await RequestAsync<Bluetooth>();
            // Возвращаем true, если оба разрешения получены
            return statusLoc == PermissionStatus.Granted && statusConnect == PermissionStatus.Granted;
#else
            return true;
#endif
        }





        //public async Task StartScanningAsync() {

        //    if (! await CheckPermissions())
        //    {

        //        await Application.Current.MainPage.DisplayAlert("Разрешения", "Bluetooth-разрешения требуются", "OK");  
        //        return;
        //    }


        //    /////////////////////////////////////////
        //    if (!_adapter.IsEnabled)
        //    {
        //        await Application.Current.MainPage.DisplayAlert("Bluetooth", "Bluetooth отключён", "OK");
        //        return;
        //    }

        //  //  await Application.Current.MainPage.DisplayAlert("Разрешения", "Bluetooth-разрешения запрошены", "OK");

        //    if (_receiver != null)
        //    {
        //        //🔹 Очистка предыдущего приёмника(если был)
        //        // Если приёмник уже существует, то мы его удаляем
        //        _context.UnregisterReceiver(_receiver);
        //        _receiver = null;
        //    }
        //    //🔹 Создание нового приёмника
        //    //_receiver = new DeviceReceiver(device =>
        //    //{
        //    //    DeviceDiscovered?.Invoke(device);
        //    //});

        //    // Что здесь происходит:
        //    //Action < DeviceInfo > — это делегат, который принимает один параметр типа DeviceInfo и ничего не возвращает(void).
        //    //Вы создаёте анонимный метод(анонимный делегат), который будет вызван, когда будет обнаружено новое Bluetooth - устройство.
        //    //Внутри этого делегата вызывается событие DeviceDiscovered, если на него кто - то подписан(!= null).
        //    // а на него подписан метод, который находиться в MainPage.xaml.cs

        //    // Создаём делегат, который будет вызываться при нахождении устройства
        //    // Внутри этого делегата вызывается событие DeviceDiscovered,
        //    // если на него кто - то подписан(!= null).

        //    // То есть:
        //    //Ваш делегат Action<DeviceInfo> action создаётся в StartScanningAsync.
        //    //Он передаётся в конструктор DeviceReceiver.
        //    //Когда в BroadcastReceiver приходит событие Bluetooth(BluetoothDevice.ActionFound), вызывается _onDeviceFound?.Invoke(deviceInfo);.
        //    //Это приводит к вызову того кода, который вы передали — а именно, событие DeviceDiscovered, на которое можно подписаться в MainPage.

        //    //            Кратко
        //    //В StartScanningAsync вы создаёте делегат action, который внутри вызывает событие DeviceDiscovered.
        //    //Делегат action передаётся в конструктор DeviceReceiver.
        //    //Внутри класса DeviceReceiver делегат сохраняется в поле _onDeviceFound.
        //    //Когда в DeviceReceiver.OnReceive найдено новое устройство, вызывается _onDeviceFound?.Invoke(deviceInfo);.
        //    //Это приводит к выполнению кода из делегата action, то есть к вызову DeviceDiscovered(device).
        //    //Все подписчики на событие DeviceDiscovered(например, обработчики в MainPage) будут вызваны.

        //    Action<DeviceInfo> action = async delegate (DeviceInfo device)
        //    {
        //        // Внутри этого делегата вызывается событие DeviceDiscovered,
        //        // если на него кто - то подписан(!= null).
        //        //"Происходит вызов события DeviceDiscovered, и все подписанные на него обработчики (в данном случае — из MainPage)
        //        //будут вызваны."

        //        //await MainThread.InvokeOnMainThreadAsync(async () =>
        //        //{
        //        //    await Application.Current.MainPage.DisplayAlert(
        //        //        "Bluetooth",
        //        //        $"Bluetooth\n Имя: {device.Name}\n address: {device.Address}",
        //        //        "OK"
        //        //    );
        //        //});


        //        if (DeviceDiscovered != null) { DeviceDiscovered(device);}
        //    };
        //    // Создаём экземпляр DeviceReceiver и передаём ему делегат
        //    _receiver = new DeviceReceiver(action);
        //    //🔹Фильтрация  приёмника на событие ACTION_FOUND
        //    IntentFilter filter = new IntentFilter(BluetoothDevice.ActionFound);
        //    //Регистрируем BroadcastReceiver, чтобы получать уведомления, когда найдено Bluetooth-устройство.
        //    _context.RegisterReceiver(_receiver, filter);

        //    _adapter.StartDiscovery();



        //}



        // Новое событие
        public event Action DiscoveryFinished;

        public async Task<bool> StartScanningAsync()
        {
           
            if (!await RequestBluetoothPermissionsAsync())
            {
                await Application.Current.MainPage.DisplayAlert("Permissions", "Bluetooth permissions required", "OK");
                return false;
            }


            if (!_adapter.IsEnabled)
            {
                await Application.Current.MainPage.DisplayAlert("Bluetooth", "Bluetooth is disabled", "OK");
                return false;
            }

            if (_receiver != null)
            {
                _context.UnregisterReceiver(_receiver);
                _receiver = null;
            }

            _receiver = new DeviceReceiver(device =>
            {
                DeviceDiscovered?.Invoke(device);
            },()=> DiscoveryFinished?.Invoke());

            IntentFilter filter = new IntentFilter(BluetoothDevice.ActionFound);
            filter.AddAction(BluetoothAdapter.ActionDiscoveryFinished); // Добавляем фильтр для события начала сканирования
            _context.RegisterReceiver(_receiver, filter);

            _adapter.StartDiscovery();

           
           return true;

        }


        public Task StopScanningAsync()
        {
            //_adapter Объект типа BluetoothAdapter, полученный ранее через BluetoothAdapter.DefaultAdapter.
            //IsDiscovering Свойство: true, если сейчас происходит поиск Bluetooth - устройств.
            if (_adapter.IsDiscovering)
                //Метод, останавливающий активный процесс сканирования.
                _adapter.CancelDiscovery();
            //_receiver	Объект типа BroadcastReceiver, обрабатывающий событие BluetoothDevice.ActionFound (устройство найдено).
            if (_receiver != null)
            {
                //Удаляет приёмник из системы, чтобы он не продолжал слушать события после остановки сканирования.
                _context.UnregisterReceiver(_receiver);
                //Освобождаем ссылку, чтобы в будущем можно было безопасно зарегистрировать новый приёмник.
                _receiver = null;
            }
        //    Application.Current.MainPage.DisplayAlert("Bluetooth", "Scanning stopped", "OK");
            //Task.CompletedTask — означает завершение без ожидания.
            return Task.CompletedTask;
        }




        public async Task Clear_Device()
        {

            try
            {
                if (_receiver != null)
                {
                    //Удаляет приёмник из системы, чтобы он не продолжал слушать события после остановки сканирования.
                    _context.UnregisterReceiver(_receiver);
                    //Освобождаем ссылку, чтобы в будущем можно было безопасно зарегистрировать новый приёмник.
                    _receiver = null;
                }
            }
            catch (Exception ex) { await Application.Current.MainPage.DisplayAlert("Error", $"{ex.Message}", "OK"); }


            if (socket_global != null) {

                try 
                { 
                    socket_global.Close();
                    socket_global = null;

                }

                catch (Exception ex) { await Application.Current.MainPage.DisplayAlert("Error", $"{ex.Message}", "OK"); }

            }
           
          await  Application.Current.MainPage.DisplayAlert("Bluetooth", "Device clear", "OK");
            //Task.CompletedTask — означает завершение без ожидания.
           
        }   

        public async Task DisconnectFromDeviceAsync()
        {
            if (_connectedDevice != null)
            {
                await Application.Current.MainPage.DisplayAlert("Disabled", $"Disabled from {_connectedDevice.Name}", "OK");
                _connectedDevice = null;
            }
        }

        public async Task OnOffBluetooth()
        {


            try
            {
                // 🔍 Проверяем, инициализирован ли адаптер Bluetooth
                //🔹 Проверка на инициализацию Bluetooth адаптера.Если _adapter == null, значит устройство не поддерживает Bluetooth
                //    или не удалось получить доступ к нему. Выводится сообщение и выход из метода.
                if (_adapter == null)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Bluetooth adapter not found", "OK");
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
                        //Когда вы используете AddFlags(ActivityFlags.NewTask) в Android, вы сообщаете системе,
                        //что создаваемый Intent должен запускаться в новой задаче(new task).Это особенно важно,
                        //если вы запускаете Intent не из контекста Activity, а из контекста Context. 
                       // Почему используется AddFlags(ActivityFlags.NewTask) ?
                            //Контекст Context vs Activity:
                       //Если вы запускаете Intent из Activity, система автоматически добавляет его в текущую задачу.
                       // если вы запускаете Intent из Context(например, из службы Service или другого компонента,
                       // не связанного с пользовательским интерфейсом), система не знает, к какой задаче привязать новый Intent.
                       // В этом случае вы должны явно указать, чтобы он запускался в новой задаче.
                        intent.AddFlags(ActivityFlags.NewTask);
                        //_context.StartActivity(intent)  Запускает созданный Intent.В данном случае — открывает настройки Bluetooth.
                        _context.StartActivity(intent);
                        ///////////////////////


                    }
                    else
                    {
                        // Android < 13: отключаем Bluetooth через рефлексию
                        try
                        {
                            //Class.GetMethod("disable")  Использует рефлексию для поиска скрытого метода disable,
                            //так как он не входит в публичный API.
                            var disableMethod = _adapter.Class.GetMethod("disable");
                            //Invoke(_adapter)  Вызывает метод disable на текущем экземпляре _adapter.
                            disableMethod?.Invoke(_adapter);
                            await Application.Current.MainPage.DisplayAlert("Bluetooth", "Bluetooth is disabled", "OK");
                        }
                        catch (Exception ex)
                        {
                            await Application.Current.MainPage.DisplayAlert("Error", $"Failed to disable Bluetooth: {ex.Message}", "OK");
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
                       // _adapter.Enable()   Пытается включить Bluetooth.Возвращает true, если команда отправлена.
                        bool enabled = _adapter.Enable();
                        string message = enabled ? "Bluetooth is on" : "Failed to turn on Bluetooth";
                        await Application.Current.MainPage.DisplayAlert("Bluetooth", message, "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Bluetooth Error: {ex.Message}", "OK");
            }

        }

        public async Task ConnectToDeviceAsync2(DeviceInfo deviceInfo)
        {
            // await Application.Current.MainPage.DisplayAlert("Выбран", $"Модуль:{deviceInfo.Name} {deviceInfo.Address}", "OK");

            try
            {
                await Application.Current.MainPage.DisplayAlert("Wait for connection", $"{deviceInfo.Name} [{deviceInfo.Address}]", "OK");
                // 🔹 Получает объект BluetoothDevice по MAC-адресу.
                // 🔹 Этот объект нужен для создания сокета и установления соединения.
                // 🔹 Получаем BluetoothDevice по MAC-адресу.
                var device = _adapter.GetRemoteDevice(deviceInfo.Address);
                // 🔹 Отменяет текущее сканирование Bluetooth-устройств, если оно активно.
                // 🔹 Это нужно, чтобы избежать конфликтов при подключении к устройству.
                _adapter.CancelDiscovery();
                // 🔹 Создаёт сокет для подключения к устройству.
                // 🔹 UUID — уникальный идентификатор, который используется для идентификации сервиса на устройстве.
                // 🔹 В данном случае используется стандартный UUID для SPP (Serial Port Profile).
                socket_global = device?.CreateRfcommSocketToServiceRecord(Java.Util.UUID.FromString("00001101-0000-1000-8000-00805F9B34FB"));
                if (socket_global == null)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to create socket", "OK");
                    return;
                }
                await Task.Run(socket_global.Connect);
                // 🔹 Если соединение успешно, то выводим сообщение об успешном подключении.

                if (socket_global.IsConnected) {

                    deviceInfo.IsConnected = true; // Помечаем устройство как подключённое
                    await Application.Current.MainPage.DisplayAlert("Success", $"Connected{device?.Name}","Ok");
                    // 🔹 Помечаем текущее устройство как подключённое

                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to connect socket", "OK");
                }


            }
            catch (Exception e)
            {

                await Application.Current.MainPage.DisplayAlert("Error", $"{e.Message}","Ок");
            }



        }


        public event Action<string> DataReceived; // 👉 событие для передачи данных в UI
        public async Task ReceiverData() 
        {

            //включение Foreground Service
            _context = Platform.AppContext;
            var intent = new Intent(_context, typeof(Bluetooth_Foregraund_service));
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
#pragma warning disable CA1416 // Проверка совместимости платформы
                _context.StartForegroundService(intent); // Android 8+
#pragma warning restore CA1416 // Проверка совместимости платформы
               else
                _context.StartService(intent); // Android < 8                  

            // Буфер для приёма "сырых" байтов из Bluetooth (4096 байт за раз).
            byte[] buffer = new byte[4096];
            // StringBuilder — для накопления текста, если сообщение приходит не целиком, а частями.
            StringBuilder dataBuffer = new StringBuilder();
            
            try
            {

                // Берём поток, с которого будем читать. Должен быть уже открыт и готов к чтению.
                var _inputStream = socket_global?.InputStream;
               

                if (_inputStream == null)
                {
                    // DataReceived?.Invoke("Ошибка: Bluetooth поток не инициализирован.");
                    // _myEvent?.Invoke("Вызов делегата: public delegate void MyEventHandler(string message);");
                   // MyEvent.Invoke("не явно реализованное событие");
                   //   return;
                }

                // Бесконечный цикл для непрерывного чтения данных, пока соединение активно.
                while (true) 
                {

                    if (!_adapter.IsEnabled) {
                        MainThread.BeginInvokeOnMainThread(() => {
                            Application.Current.MainPage.DisplayAlert("Bluetooth", "Bluetooth is disabled", "OK");
                        });
                        socket_global?.Close(); // Закрываем сокет, если адаптер не инициализирован
                        _context.UnregisterReceiver(_receiver);
                        return;
                    }


                    // Делаем небольшую паузу, чтобы не грузить процессор.
                    await Task.Delay(100);
                    // Проверяем, можно ли читать из потока (соединение не закрыто и поток поддерживает чтение)
                    if (_inputStream.CanRead) 
                    {
                        // Читаем данные из потока в буфер.
                        int bytesRead = await _inputStream.ReadAsync(buffer, 0, buffer.Length);
                        // Если что-то действительно прочитали...
                        if (bytesRead > 0) 
                        {
                            // Преобразуем байты в строку (ASCII).
                            string part = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                            // Добавляем прочитанную часть к накопленному тексту.
                            dataBuffer.Append(part);
                            // Если в пришедшей части есть символ новой строки, значит сообщение завершено.
                            if (part.Contains("\n")) 
                            {

                                // Собираем полное сообщение, убираем лишние пробелы.
                                string completeMessage = dataBuffer.ToString().Trim();
                                // 👉 Передаем полученную строку через событие DataReceived.
                                // Если в MainPage подписка на это событие — она получит сообщение и обновит label4.
                                DataReceived?.Invoke(completeMessage);
                             //   if (DataReceived != null) { DataReceived(completeMessage); }
                                // Очищаем буфер, чтобы начать накопление следующего сообщения.
                                dataBuffer.Clear();
                            }

                        }


                    }



                }

            }
            catch (Exception ex)
            {

                // Если возникла ошибка (например, разрыв соединения),
                // отправляем сообщение об ошибке через то же событие в UI.
                DataReceived?.Invoke($"Error: {ex.Message}");
            }



        }


        public event MyEventHandler MyEvent;
        public async Task ClearData()
        {
            //Запуск события
            MyEvent.Invoke();
        
        
        }

        public event MyEventHandler_T MyEvent_T;

        public async Task TransmitterData(string data)
        {

           // await Application.Current.MainPage.DisplayAlert("Передача данных", "Данные переданы", "OK");
           
           
            try
            {
               
                await Task.Delay(100); // Небольшая пауза, чтобы не нагружать процессор
                byte[] buffer = Encoding.ASCII.GetBytes(data); // Преобразуем строку в массив байтов
                MyEvent_T?.Invoke(buffer); // Вызываем событие, передавая в него буфер
             //   await Application.Current.MainPage.DisplayAlert("Передача данных", "Данные переданы", "OK");


            }
            catch (Exception ex)
            {

               await Application.Current.MainPage.DisplayAlert($"Error - {ex.Message}", "Failed to transfer data", "OK");
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


/////////////////////////////////////////////////////////
///НАЧАЛО
//  |
//  V
//Адаптер Bluetooth доступен?
//  └─ Нет → "Ошибка" → КОНЕЦ
//  |
//  Да
//  |
//Bluetooth ВКЛЮЧЕН?
//  └─ Да
//      └─ Android 13+?
//           ├─ Да → Открыть настройки Bluetooth
//           └─ Нет → Отключить Bluetooth через рефлексию
//  └─ Нет
//      └─ Android 13+?
//           ├─ Да → Открыть настройки Bluetooth
//           └─ Нет → Включить Bluetooth через _adapter.Enable()





////////////////////////////////////////////////////
///
//// Получаем поток ввода из сокета.
//// socket_global_InputStream = socket_global.InputStream; // Инициализируй его при подключении!
//// Читаем данные из потока в бесконечном цикле.
//while (true)
//{
//    // Читаем данные из потока. Метод Read() блокирует выполнение, пока не получит данные.
//    int bytesRead = await socket_global.InputStream.ReadAsync(buffer, 0, buffer.Length);
//    if (bytesRead > 0)
//    {
//        // Преобразуем байты в строку и добавляем к накопленному тексту.
//        string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
//        dataBuffer.Append(data);
//        // Вызываем событие DataReceived с накопленным текстом.
//        DataReceived?.Invoke(dataBuffer.ToString());
//    }
//}

//Permission permission_BluetoothScan = AndroidX.Core.Content.ContextCompat.CheckSelfPermission(_context, Manifest.Permission.BluetoothScan);
//Permission permission_BluetoothConnect = AndroidX.Core.Content.ContextCompat.CheckSelfPermission(_context, Manifest.Permission.BluetoothConnect);
//Permission permission_AccessFineLocation = AndroidX.Core.Content.ContextCompat.CheckSelfPermission(_context, Manifest.Permission.AccessFineLocation);
//string [] permission_BluetoothScanString  = { Manifest.Permission.BluetoothScan,Manifest.Permission.BluetoothConnect,Manifest.Permission.AccessFineLocation };

//if (Build.VERSION.SdkInt >= BuildVersionCodes.S) 
//{
//    if ( permission_BluetoothScan != Permission.Granted)
//    {  ActivityCompat.RequestPermissions(Platform.CurrentActivity, new [] { Manifest.Permission.BluetoothScan }, 0);
//        await Application.Current.MainPage.DisplayAlert("Разрешения", "Bluetooth-разрешения требуются (Android 12+)", "OK");
//    }
//    if (permission_BluetoothConnect != Permission.Granted)
//    { ActivityCompat.RequestPermissions(Platform.CurrentActivity, new[] { Manifest.Permission.BluetoothConnect }, 0);
//       await Application.Current.MainPage.DisplayAlert("Разрешения", "Bluetooth-разрешения требуются (Android 12+)", "OK");
//    }


//}
//else {

//    if (permission_AccessFineLocation != Permission.Granted)
//    {
//        ActivityCompat.RequestPermissions(Platform.CurrentActivity, new[] { Manifest.Permission.AccessFineLocation }, 0);
//        await Application.Current.MainPage.DisplayAlert("Разрешения", "Bluetooth-разрешения требуются (Android < 12)", "OK");
//    }



//}

//#if ANDROID
//            // Запрашиваем разрешение на использование геолокации (требуется для поиска Bluetooth-устройств в Android)
//            var statusLoc = await RequestAsync<LocationWhenInUse>();
//            // Запрашиваем разрешение на использование Bluetooth
//            var statusConnect = await RequestAsync<Bluetooth>();
//            // Возвращаем true, если оба разрешения получены
//            return statusLoc == PermissionStatus.Granted && statusConnect == PermissionStatus.Granted;
//#else
//            return true;
//#endif



//if (Build.VERSION.SdkInt >= BuildVersionCodes.S) // Android 12+
//{
//    var permissionsToRequest = new List<string>();
//    if (ContextCompat.CheckSelfPermission(Platform.CurrentActivity, Manifest.Permission.BluetoothScan) != Permission.Granted)
//        permissionsToRequest.Add(Manifest.Permission.BluetoothScan);

//    if (ContextCompat.CheckSelfPermission(Platform.CurrentActivity, Manifest.Permission.BluetoothConnect) != Permission.Granted)
//        permissionsToRequest.Add(Manifest.Permission.BluetoothConnect);

//    if (ContextCompat.CheckSelfPermission(Platform.CurrentActivity, Manifest.Permission.AccessFineLocation) != Permission.Granted)
//        permissionsToRequest.Add(Manifest.Permission.AccessFineLocation);

//    if (permissionsToRequest.Any())
//        ActivityCompat.RequestPermissions(Platform.CurrentActivity, permissionsToRequest.ToArray(), 102);
//}
//else // Android 11 и ниже
//{
//    var permissionsToRequest = new List<string>();
//    if (ContextCompat.CheckSelfPermission(Platform.CurrentActivity, Manifest.Permission.Bluetooth) != Permission.Granted)
//        permissionsToRequest.Add(Manifest.Permission.Bluetooth);

//    if (ContextCompat.CheckSelfPermission(Platform.CurrentActivity, Manifest.Permission.AccessFineLocation) != Permission.Granted)
//        permissionsToRequest.Add(Manifest.Permission.AccessFineLocation);

//    if (permissionsToRequest.Any())
//        ActivityCompat.RequestPermissions(Platform.CurrentActivity, permissionsToRequest.ToArray(), 102);
//}

/////////////////////////////////

// Android 12 и выше (API 31+)
//if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
//{
//    var permissions = new[]
//    {
//        Manifest.Permission.BluetoothScan,    // — разрешение на поиск Bluetooth-устройств, появилось в Android 12.
//        Manifest.Permission.BluetoothConnect  // - — разрешение на подключение к Bluetooth-устройствам, появилось в Android 12.
//    };

//    foreach (var permission in permissions)
//    {
//        if (ContextCompat.CheckSelfPermission(Platform.CurrentActivity, permission) != Permission.Granted)
//        {
//            ActivityCompat.RequestPermissions(Platform.CurrentActivity, permissions, 0);
//            await Application.Current.MainPage.DisplayAlert("Разрешения", "Bluetooth-разрешения требуются (Android 12+)", "OK");
//            return;
//        }

//    }
//}
//else // Android 6–11 (API 23–30)
//{
//    var legacyPermission = Manifest.Permission.AccessFineLocation;

//    if (ContextCompat.CheckSelfPermission(Platform.CurrentActivity, legacyPermission) != Permission.Granted)
//    {
//        ActivityCompat.RequestPermissions(Platform.CurrentActivity, new[] { legacyPermission }, 0);
//        await Application.Current.MainPage.DisplayAlert("Разрешения", "Разрешение на геолокацию требуется (Android < 12)", "OK");
//        return;
//    }
//}


