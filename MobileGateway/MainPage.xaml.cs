#if ANDROID
using Android.Bluetooth;
using Android.Content;
using MobileGateway.Platforms.Android.Services;
using Application = Android.App.Application;
#endif

namespace MobileGateway;

public partial class MainPage : ContentPage
{
    public class BluetoothDeviceModel
    {
        public string? Name { get; set; }
        public required string Id { get; set; }
    }

    private List<BluetoothDeviceModel> _classicDevices = new();

#if ANDROID
    private BluetoothAdapter? _adapter;
    private List<BluetoothDeviceModel> _bondedDevices = new();
    private List<BluetoothDeviceModel> _connectedDevices = new();
#endif

    private readonly IServiceProvider _serviceProvider;

    public MainPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;

        var username = Preferences.Get("Username", null);
        if (!string.IsNullOrEmpty(username))
        {
            welcomeLabel.Text = $"Привіт, {username}!";
        }
    }


    private void OnLogoutClicked(object sender, EventArgs e)
    {
        Preferences.Remove("UserToken");
        Preferences.Remove("Username");

        Microsoft.Maui.Controls.Application.Current.MainPage = _serviceProvider.GetRequiredService<LoginPage>();
    }


    private async void OnScanClicked(object sender, EventArgs e)
    {
#if ANDROID
        var context = Application.Context;

        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.S)
        {
            var permission = Android.Content.PM.Permission.Granted;
            if (AndroidX.Core.Content.ContextCompat.CheckSelfPermission(context, Android.Manifest.Permission.BluetoothScan) != permission ||
                AndroidX.Core.Content.ContextCompat.CheckSelfPermission(context, Android.Manifest.Permission.BluetoothConnect) != permission)
            {
                var activity = Platform.CurrentActivity;
                AndroidX.Core.App.ActivityCompat.RequestPermissions(activity, new[] {
                    Android.Manifest.Permission.BluetoothScan,
                    Android.Manifest.Permission.BluetoothConnect
                }, 1001);
                statusLabel.Text = "Очікування дозволу на Bluetooth...";
                return;
            }
        }

        _adapter = BluetoothAdapter.DefaultAdapter;
        if (_adapter == null)
        {
            await DisplayAlert("Bluetooth", "Bluetooth недоступний на цьому пристрої.", "OK");
            return;
        }

        if (!_adapter.IsEnabled)
        {
            await DisplayAlert("Bluetooth", "Увімкніть Bluetooth для пошуку пристроїв.", "OK");
            return;
        }

        _bondedDevices.Clear();
        _connectedDevices.Clear();
        bondedDevicesView.ItemsSource = null;
        connectedDevicesView.ItemsSource = null;

        // Зв'язані пристрої
        _bondedDevices.AddRange(
            from device in _adapter.BondedDevices
            select new BluetoothDeviceModel
            {
                Name = string.IsNullOrEmpty(device.Name) ? "Невідомий" : device.Name,
                Id = device.Address
            });
        bondedDevicesView.ItemsSource = _bondedDevices;

        // Підключення до A2DP профілю через Proxy
        _adapter.GetProfileProxy(context, new A2dpServiceListener(connectedDevicesView, statusLabel), ProfileType.A2dp);

        statusLabel.Text = $"Зв'язано: {_bondedDevices.Count} (очікуємо A2DP...)";

#else
        await DisplayAlert("Bluetooth", "Bluetooth доступний лише на Android.", "OK");
#endif
    }
}
