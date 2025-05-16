using Android.Bluetooth;
using Android.Content;
using DeviceInfo = Project_Bluetooth.Models.DeviceInfo;


//using Project_Bluetooth.Platforms.Models;

namespace Project_Bluetooth.Platforms.Android
{
    // Получает найденные Bluetooth-устройства
    public class DeviceReceiver : BroadcastReceiver
    {
        private readonly Action<DeviceInfo> _onDeviceFound;

        // В конструктор передаётся делегат, который будет вызываться при нахождении устройства
        public DeviceReceiver(Action<DeviceInfo> onDeviceFound)
        {
            _onDeviceFound = onDeviceFound;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action == BluetoothDevice.ActionFound)
            {
                var device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                if (device != null)
                {
                    var name = string.IsNullOrEmpty(device.Name) ? "Неизвестное устройство" : device.Name;
                    var address = device.Address;

                    var deviceInfo = new DeviceInfo
                    {
                        Name = name,
                        Address = address
                    };

                   
                    // Вызываем делегат с найденным устройством
                    _onDeviceFound?.Invoke(deviceInfo);
                }
            }
        }
    }
}
