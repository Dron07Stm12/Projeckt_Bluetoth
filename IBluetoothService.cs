namespace Project_Bluetooth
{
    public interface IBluetoothService
    {
        Task StartScanningAsync();
        Task StopScanningAsync();
        Task ConnectToDeviceAsync(string deviceAddress);
        Task DisconnectFromDeviceAsync();

        Task OnOffBluetoothAsyncc();

    }
}