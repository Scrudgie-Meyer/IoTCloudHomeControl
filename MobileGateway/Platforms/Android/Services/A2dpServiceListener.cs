using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Bluetooth;
using Android.Runtime;
using static MobileGateway.MainPage;

namespace MobileGateway.Platforms.Android.Services
{
    public class A2dpServiceListener : Java.Lang.Object, IBluetoothProfileServiceListener
    {
        private readonly CollectionView _view;
        private readonly Label _status;

        public A2dpServiceListener(CollectionView view, Label status)
        {
            _view = view;
            _status = status;
        }

        public void OnServiceConnected([GeneratedEnum] ProfileType profile, IBluetoothProfile proxy)
        {
            if (profile == ProfileType.A2dp)
            {
                var devices = proxy.ConnectedDevices;
                var list = new List<BluetoothDeviceModel>();

                foreach (var device in devices)
                {
                    var deviceClass = device.BluetoothClass;
                    if (deviceClass != null && deviceClass.MajorDeviceClass == MajorDeviceClass.AudioVideo)
                    {
                        list.Add(new BluetoothDeviceModel
                        {
                            Name = string.IsNullOrEmpty(device.Name) ? "Аудіопристрій" : device.Name,
                            Id = device.Address
                        });
                    }
                }

                Microsoft.Maui.Controls.Application.Current.Dispatcher.Dispatch(() =>
                {
                    _view.ItemsSource = list;
                    _status.Text += $" | Підключено A2DP: {list.Count}";
                });

                BluetoothAdapter.DefaultAdapter.CloseProfileProxy(ProfileType.A2dp, proxy);
            }
        }

        public void OnServiceDisconnected([GeneratedEnum] ProfileType profile)
        {
            // Можна додати логіку на втрату сервісу, якщо треба
        }
    }

}
