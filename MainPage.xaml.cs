using System;
using System.Collections.ObjectModel;
using Project_Bluetooth.Models;
using DeviceInfo = Project_Bluetooth.Models.DeviceInfo;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;



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

        //поле для хранения значений для отображения в сетке 16x8 (128 ячеек)
        private double?[] _gridValues = new double?[128];


        public MainPage(IBluetoothService bluetoothService)
        {
            InitializeComponent();
            _bluetoothService = bluetoothService;
            //связывает XAML-интерфейс с текущим классом (например, MainPage), чтобы иметь доступ к свойствам и командам напрямую из XAML.
            BindingContext = this;

            ShowPage(2);

            for (int i = 0; i < 128; i++)
                _gridValues[i] = null;

            grid128View.Drawable = new Grid128Drawable(_gridValues);










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

            // Подписываемся на событие получения данных из Bluetooth-сервиса
            bluetoothService.DataReceived += OnDataReceived;
            // Подписываемся на событие MyEvent из Bluetooth-сервиса
            bluetoothService.MyEvent += OnDataClear;
            // Подписываемся на событие MyEvent_T из Bluetooth-сервиса
            //  bluetoothService.MyEvent_T += OnMyEvent_T;
            // Подписываемся на событие DiscoveryFinished из Bluetooth-сервиса
            bluetoothService.DiscoveryFinished += delegate ()
            {
                // Главный поток для UI, чтоб в UI было изменение коллекции DiscoveredDevices
                MainThread.BeginInvokeOnMainThread(delegate ()
                {
                    // обработка крутилки
                    activityIndicator.IsVisible = false;//элемент виден пользователю.
                    activityIndicator.IsRunning = false;//индикатор крутится
                });
            };

#if ANDROID
            // Подписываемся на событие MyEvent из Bluetooth-сервиса
            // Это проверка типа: мы проверяем, является ли внедрённый сервис _bluetoothService
            // конкретной реализацией AndroidBluetoothService.
            // Если это так, то приводим его к этому типу и сохраняем в переменную androidService.
            // В этом случае мы можем использовать специфичные для Android методы и свойства.
            // например, socket_global.

            if (_bluetoothService is AndroidBluetoothService androidService)
            {
               // byte[] buffer = new byte[4096];

                androidService.MyEvent_T +=  delegate (byte[] buffer)
                {
                      // Подписываемся на событие MyEvent_T.
                      // Когда событие будет вызвано, выполнится этот анонимный обработчик.
                    // Главный поток для UI, чтоб в UI было изменение коллекции DiscoveredDevices
                    MainThread.BeginInvokeOnMainThread( async delegate ()
                    {
                        // Выводим сообщение
                   //  await DisplayAlert("MyEvent", "главный_поток", "OK");

                      if (androidService.socket_global != null && androidService.socket_global.IsConnected)
                        {
                            // Преобразуем строку в массив байтов
                        //    buffer = System.Text.Encoding.ASCII.GetBytes(entry1.Text);
                            // Отправляем данные на устройство
                           await  Task.Delay(100);
                          await  androidService.socket_global.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                           
                        
                        }
                      else
                        {
                            await  DisplayAlert("Error", "No connection to device!", "OK");
                        }

                    });
                };
            }

#endif

        }


        //


        private int _currentPage = 1;

//        private void ShowPage(int page)
//        {
//            _currentPage = page;

//#if ANDROID
//            Page1Block.IsVisible = page == 1;
//            Page2Block.IsVisible = page == 2;

//            Page1Indicator.TextColor = page == 1 ? Colors.White : Colors.Black;
//            Page2Indicator.TextColor = page == 2 ? Colors.White : Colors.Black;

//#endif
//        }


        private void ShowPage(int page)
        {
            _currentPage = page;

            Page1Block.IsVisible = page == 1;
            Page2Block.IsVisible = page == 2;

            Page1Indicator.TextColor = page == 1 ? Colors.White : Colors.Gray;
            Page2Indicator.TextColor = page == 2 ? Colors.White : Colors.Gray;

            DebugLabelButton.TextColor = page == 1 ? Colors.White : Colors.Gray;
            UserLabelButton.TextColor = page == 2 ? Colors.White : Colors.Gray;
        }





        private void ShowPage1Clicked(object sender, EventArgs e)
        {
            ShowPage(1);
        }

        private void ShowPage2Clicked(object sender, EventArgs e)
        {
            ShowPage(2);
        }

        private void OnSwipedLeft(object sender, SwipedEventArgs e)
        {
            if (_currentPage == 1)
                ShowPage(2);
        }

        private void OnSwipedRight(object sender, SwipedEventArgs e)
        {
            if (_currentPage == 2)
                ShowPage(1);
        }

        private async void OnAction1Clicked(object sender, EventArgs e)
        {
            await DisplayAlert("Second screen", "Action 1 pressed", "OK");
        }

        private async void OnAction2Clicked(object sender, EventArgs e)
        {
            await DisplayAlert("Second screen", "Action 2 pressed", "OK");
        }









        //



        protected override void OnAppearing()
        {
            base.OnAppearing();
            #if ANDROID || IOS
                Shell.SetNavBarIsVisible(this, false);
            #endif
        }



        private async void OnClearDevices(object s,EventArgs args) { await _bluetoothService.ClearData(); }

        private async void TransmitterData(object s, EventArgs args) {
            
            string data = entry1.Text;
            await  DisplayAlert("Data", $"TransmitterData: {data}", "OK");
            await _bluetoothService.TransmitterData(data);
        }

        //обработчик
        private void OnDataClear() {

            // Выводим сообщение
            MainThread.BeginInvokeOnMainThread(() =>
            {
                DisplayAlert("Data", "Clear", "OK");
                label4.Text = "";
            });

        }




        // Обработчик события DataReceived: обновляет label4 в UI потоке
        // Обновление label4 только на главном потоке!
        //private void OnDataReceived(string message)
        //{
        //    MainThread.BeginInvokeOnMainThread(() =>
        //    {
        //        label4.Text += $"{Environment.NewLine}{message}";
        //        scrollView.ScrollToAsync(label4, ScrollToPosition.End, true);
        //    });
        //}


        //private void UpdateGridCellFromMessage(string message)
        //{
        //    if (string.IsNullOrWhiteSpace(message))
        //        return;

        //    // Пример строки:
        //    // R00 = 0.72 v - IDLE_SENSOR

        //    try
        //    {
        //        string trimmed = message.Trim();

        //        if (!trimmed.StartsWith("R"))
        //            return;

        //        int eqIndex = trimmed.IndexOf('=');
        //        if (eqIndex < 0)
        //            return;

        //        string sensorPart = trimmed.Substring(0, eqIndex).Trim();   // R00
        //        string valuePart = trimmed.Substring(eqIndex + 1).Trim();   // 0.72 v - IDLE_SENSOR

        //        // Берём индекс из R00 -> 00
        //        string indexText = sensorPart.Replace("R", "").Trim();

        //        if (!int.TryParse(indexText, out int cellIndex))
        //            return;

        //        if (cellIndex < 0 || cellIndex >= 128)
        //            return;

        //        // Берём число до "v"
        //        int vIndex = valuePart.IndexOf('v');
        //        string voltageText;

        //        if (vIndex >= 0)
        //            voltageText = valuePart.Substring(0, vIndex).Trim();
        //        else
        //            voltageText = valuePart;

        //        if (double.TryParse(
        //                voltageText,
        //                System.Globalization.NumberStyles.Any,
        //                System.Globalization.CultureInfo.InvariantCulture,
        //                out double voltage))
        //        {
        //            _gridValues[cellIndex] = voltage;

        //            grid128View.Drawable = new Grid128Drawable(_gridValues);
        //            grid128View.Invalidate();
        //        }
        //    }
        //    catch
        //    {
        //        // можно пока ничего не делать
        //    }
        //}


        private void UpdateGridCellsFromMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            var lines = message.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

            bool changed = false;

            foreach (var line in lines)
            {
                if (TryParseGridCell(line, out int cellIndex, out double voltage))
                {
                    _gridValues[cellIndex] = voltage;
                    changed = true;
                }
            }

            if (changed)
            {
                grid128View.Invalidate();
            }
        }



        private bool TryParseGridCell(string message, out int cellIndex, out double voltage)
        {
            cellIndex = -1;
            voltage = 0;

            if (string.IsNullOrWhiteSpace(message))
                return false;

            try
            {
                string trimmed = message.Trim();

                if (!trimmed.StartsWith("R"))
                    return false;

                int eqIndex = trimmed.IndexOf('=');
                if (eqIndex < 0)
                    return false;

                string sensorPart = trimmed.Substring(0, eqIndex).Trim();   // R00
                string valuePart = trimmed.Substring(eqIndex + 1).Trim();   // 0.72 v - IDLE_SENSOR

                string indexText = sensorPart.Replace("R", "").Trim();

                if (!int.TryParse(indexText, out cellIndex))
                    return false;

                if (cellIndex < 0 || cellIndex >= 128)
                    return false;

                int vIndex = valuePart.IndexOf('v');
                string voltageText = vIndex >= 0
                    ? valuePart.Substring(0, vIndex).Trim()
                    : valuePart;

                return double.TryParse(
                    voltageText,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out voltage);
            }
            catch
            {
                return false;
            }
        }




        private void OnDataReceived(string message)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                label4.Text += $"{Environment.NewLine}{message}";
                scrollView.ScrollToAsync(label4, ScrollToPosition.End, true);

                UpdateGridCellsFromMessage(message);
            });
        }






        // Вызов приёма данных по Bluetooth по нажатию кнопки (Receiver)
        private async void DataReciever(object sender, EventArgs e)
        {
            await _bluetoothService.ReceiverData();
        }
        private async void On_Off_Bluetooth(object sender, EventArgs e)
        {

            await _bluetoothService.OnOffBluetooth();
           
        }

        private async void OnStartScanClicked(object sender, EventArgs e)
        {
            
            bool started = await _bluetoothService.StartScanningAsync();

            if (started) {
                // обработка крутилки
                MainThread.BeginInvokeOnMainThread(() =>
                {

                    activityIndicator.IsVisible = true;//элемент виден пользователю.
                    activityIndicator.IsRunning = true;//индикатор крутится
                });

            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {

                    activityIndicator.IsVisible = false;//элемент виден пользователю.
                    activityIndicator.IsRunning = false;//индикатор крутится
                });

            }

        }

      

        private async void OnStopScanClicked(object sender, EventArgs e)
        {
            // обработка крутилки
            MainThread.BeginInvokeOnMainThread(() =>
            {

                activityIndicator.IsVisible = false;//элемент виден пользователю.
                activityIndicator.IsRunning = false;//индикатор крутится
            });

            DiscoveredDevices.Clear();
            await _bluetoothService.Clear_Device();

        }

        //private async void Disconnect(object sender, EventArgs e)
        //{
            
        //}


        private async void OnDeviceTapped(object sender, TappedEventArgs e)
        {
           
            

            if (sender is Border border) {
                if (border.BindingContext is DeviceInfo device_info ) {
                    await DisplayAlert("Device Selection", $" {device_info.Name} {device_info.Address}", "OK");
                    await _bluetoothService.ConnectToDeviceAsync2(device_info);

                    if (device_info.IsConnected) {

                       

                        Func<DeviceInfo, bool> predicate = delegate (DeviceInfo deviceInfo2)
                        {
                            return deviceInfo2.Address != device_info.Address;
                        };

                        var  devicesToRemove2 = DiscoveredDevices
                            .Where(predicate)
                            .ToList();

                        foreach (var device in devicesToRemove2)
                            {
                            DiscoveredDevices.Remove(device);
                        }




                    }
                
                
                
                }
            }

           
        }


        private async void OnPrivacyPolicyClicked(object sender, EventArgs e)
        {
            await Launcher.Default.OpenAsync("https://docs.google.com/document/d/1cyXZfx0MQ9GGW2_5GEabZeNPW84Ipgp3vBGMGTwf2Vc/edit?tab=t.0");
        }




    }

    //////

    public class Grid128Drawable : IDrawable
    {
        private readonly double?[] data;

        public Grid128Drawable(double?[] values)
        {
            data = values ?? new double?[128];
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            int cols = 16;
            int rows = 8;
            int totalCells = rows * cols;

            float padding = 10f;
            float spacing = 2f;

            float availableWidth = dirtyRect.Width - padding * 2;
            float availableHeight = dirtyRect.Height - padding * 2;

            float cellWidth = (availableWidth - spacing * (cols - 1)) / cols;
            float cellHeight = (availableHeight - spacing * (rows - 1)) / rows;

            canvas.StrokeSize = 1;
            canvas.FontSize = 11;
            canvas.FontColor = Colors.Black;

            for (int i = 0; i < totalCells; i++)
            {
                int row = i / cols;
                int col = i % cols;

                float x = padding + col * (cellWidth + spacing);
                float y = padding + row * (cellHeight + spacing);

                double? value = i < data.Length ? data[i] : null;

                Color fillColor;
                if (value.HasValue)
                    fillColor = GetColorByValue(value.Value);
                else
                    fillColor = Colors.LightSteelBlue;

                canvas.FillColor = fillColor;
                canvas.FillRoundedRectangle(x, y, cellWidth, cellHeight, 3);

                canvas.StrokeColor = Colors.White;
                canvas.DrawRoundedRectangle(x, y, cellWidth, cellHeight, 3);
                // $"R{i:D2}",
                canvas.DrawString(
                    $"{i:D2}",
                    x,
                    y,
                    cellWidth,
                    cellHeight,
                    HorizontalAlignment.Center,
                    VerticalAlignment.Center);
            }
        }

        private Color GetColorByValue(double v)
        {
            if (v < 0.4)
                return Colors.Gray;      // отсутствие датчика
            else if (v <= 1.01)
                return Colors.Green;     // IDLE_SENSOR
            else if (v < 2.0)
                return Colors.Peru;    // промежуточное состояние
            else if (v <= 2.91)
                return Colors.Tomato;    // ALERT
            else if (v < 3.5)
                return Colors.DarkRed;       // SHORT

            return Colors.Black;
        }
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




//private async void OnDeviceTapped(object sender, TappedEventArgs e)
//{
//    ////📌 Первая часть: sender is Border border
//    ////sender — это объект, по которому кликнули(его тип: object).
//    ////is Border border:
//    ////Проверяет, является ли sender объектом класса Border.
//    ////Если да, то:
//    ////возвращает true,
//    ////и создаёт переменную border с типом Border, ссылающуюся на sender.
//    ////🔧 Это проверка и приведение типа одновременно — без использования as + null - проверки.
//    //Border border1 = sender as Border;
//    //if (border1 != null) {
//    //  await DisplayAlert("Проверка", $"Border border1 = {sender} as Border", "OK");
//    //    //📌 Вторая часть: border.BindingContext is DeviceInfo device
//    //    //После того как border успешно получен,
//    //    //BindingContext — это объект, привязанный к Border(данные, которые отображаются).
//    //    //is DeviceInfo device:
//    //    //Проверяет, является ли этот объект экземпляром DeviceInfo.
//    //    //Если да, то:
//    //    //создаёт переменную device типа DeviceInfo,
//    //    //и ты можешь использовать её дальше в коде.
//    //    DeviceInfo device2 = border1.BindingContext as DeviceInfo;                  
//    //    if (device2 != null) {
//    //        await DisplayAlert("Проверка", $"DeviceInfo device2 = {border1.BindingContext} as DeviceInfo", "OK");
//    //    }

//    //}

//    //@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

//    //if (sender is Border border && border.BindingContext is DeviceInfo device)
//    //{
//    //   // await DisplayAlert("Выбор устройства", $"Подключение к {device.Name} ({device.Address})", "OK");

//    //    await _bluetoothService.ConnectToDeviceAsync(device.Address);
//    //}



//    if (sender is Border border)
//    {
//        if (border.BindingContext is DeviceInfo device_info)
//        {
//            await DisplayAlert("Выбор устройства", $" {device_info.Name} ({device_info.Address}", "OK");
//            await _bluetoothService.ConnectToDeviceAsync2(device_info);
//        }
//    }


//}


