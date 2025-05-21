using Project_Bluetooth.Models;
using DeviceInfo = Project_Bluetooth.Models.DeviceInfo;

//public delegate void MyEventHandler(string message); // 👈 Вынеси из интерфейса
public delegate void MyEventHandler();
public delegate void MyEventHandler_T(); // 👈 Вынеси из интерфейса

namespace Project_Bluetooth
{

   

    public interface IBluetoothService
    {

     
        event MyEventHandler MyEvent;

        event MyEventHandler_T MyEvent_T; // 👈 Вынеси из интерфейса
        // Событие: вызывается при получении строки данных из Bluetooth (например, строки с датчика)
        event Action<string> DataReceived;

        Task StartScanningAsync();
        Task StopScanningAsync();
        
        Task ConnectToDeviceAsync2(DeviceInfo deviceInfo);
        Task DisconnectFromDeviceAsync();
        Task OnOffBluetooth();

        Task ReceiverData();

        Task ClearData();

        Task TransmitterData();

    }
}