using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;

namespace MobileGateway;

public partial class MainPage : ContentPage
{
    private readonly IBluetoothLE _bluetoothLE;
    private readonly IAdapter _adapter;
    private readonly List<IDevice> _deviceList = new();

    public MainPage()
    {
        InitializeComponent();

        _bluetoothLE = CrossBluetoothLE.Current;
        _adapter = CrossBluetoothLE.Current.Adapter;
        _adapter.DeviceDiscovered += OnDeviceDiscovered;
    }

    private async void OnScanClicked(object sender, EventArgs e)
    {
        _deviceList.Clear();
        devicesView.ItemsSource = null;

        // Перевіряємо дозволи для Bluetooth та Location (для Android)
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Permissions", "Bluetooth permissions are required.", "OK");
                return;
            }
        }

        if (!_bluetoothLE.IsAvailable)
        {
            await DisplayAlert("Bluetooth", "Bluetooth недоступний на цьому пристрої.", "OK");
            return;
        }

        if (!_bluetoothLE.IsOn)
        {
            await DisplayAlert("Bluetooth", "Увімкніть Bluetooth.", "OK");
            return;
        }

        statusLabel.Text = "Пошук пристроїв...";
        try
        {
            await _adapter.StartScanningForDevicesAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка", ex.Message, "OK");
        }
        finally
        {
            statusLabel.Text = "Сканування завершено.";
            devicesView.ItemsSource = _deviceList;
        }
    }

    private void OnDeviceDiscovered(object sender, DeviceEventArgs args)
    {

        if (!_deviceList.Any(d => d.Id == args.Device.Id))
        {
            _deviceList.Add(args.Device);
        }
    }
}
