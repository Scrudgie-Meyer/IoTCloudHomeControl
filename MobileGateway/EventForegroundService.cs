#if ANDROID
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Application = Android.App.Application;

namespace MobileGateway
{
    [Service(Exported = true, ForegroundServiceType = ForegroundService.TypeDataSync)]
    public class EventForegroundService : Service
    {
        private bool _isRunning = false;
        private List<ScheduledEvent> _cachedEvents = new();
        private HashSet<int> _shownEventIds = new();

        public override void OnCreate()
        {
            base.OnCreate();
            StartForegroundServiceNotification();
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            if (!_isRunning)
            {
                _isRunning = true;
                Task.Run(BackgroundLoop);
            }

            return StartCommandResult.Sticky;
        }

        public override IBinder OnBind(Intent intent) => null;

        public override void OnDestroy()
        {
            _isRunning = false;
            base.OnDestroy();
        }

        private void StartForegroundServiceNotification()
        {
            string channelId = "event_service_channel";
            string channelName = "Сервіс подій";
            var manager = (NotificationManager)GetSystemService(NotificationService);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(channelId, channelName, NotificationImportance.Low);
                manager.CreateNotificationChannel(channel);
            }

            var notification = new Notification.Builder(this)
                .SetContentTitle("Сервіс активний")
                .SetContentText("Очікування подій кожні 30 хв.")
                .SetSmallIcon(Android.Resource.Drawable.IcDialogInfo)
                .SetOngoing(true)
                .SetChannelId(channelId)
                .Build();

            StartForeground(1001, notification);
        }

        private async Task BackgroundLoop()
        {
            DateTime lastApiCheck = DateTime.MinValue;

            while (_isRunning)
            {
                var now = DateTime.Now;

                // Оновлюємо список подій раз на 30 хв
                if ((now - lastApiCheck) >= TimeSpan.FromMinutes(30))
                {
                    if (IsInternetAvailable())
                    {
                        var newEvents = await LoadEventsFromServer();
                        if (newEvents != null)
                        {
                            _cachedEvents = newEvents;
                            _shownEventIds.Clear(); // Скидаємо ID, бо список новий
                        }

                        lastApiCheck = now;
                    }
                }

                // Перевіряємо локально, чи настав час для подій
                foreach (var e in _cachedEvents)
                {
                    if (e.IsEnabled &&
                        e.ScheduledTime <= now &&
                        !_shownEventIds.Contains(e.Id))
                    {
                        ShowPushNotification($"Подія: {e.DeviceName}", e.Description);
                        _shownEventIds.Add(e.Id);

                        if (e.IsRecurring && e.RecurrenceInterval.HasValue)
                        {
                            e.ScheduledTime = e.ScheduledTime.Add(e.RecurrenceInterval.Value);
                            _shownEventIds.Remove(e.Id); // Дозволити ще раз у майбутньому
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(1)); // Перевірка кожну хвилину, без HTTP
            }
        }


        private async Task<List<ScheduledEvent>?> LoadEventsFromServer()
        {
            try
            {
                using var client = new HttpClient();
                string? token = Preferences.Get("UserToken", null);
                if (string.IsNullOrEmpty(token)) return null;
                string? deviceSerialNumber = $"{DeviceInfo.Platform}-{DeviceInfo.Model}-{DeviceInfo.Manufacturer}";

                var apiUrl = "https://ec2-51-21-255-211.eu-north-1.compute.amazonaws.com/server" +
                             $"/api/Scenario/user/{token}/device/{deviceSerialNumber}/events";

                var response = await client.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode)
                    return null;

                var content = await response.Content.ReadAsStringAsync();
                var events = System.Text.Json.JsonSerializer.Deserialize<List<ScheduledEvent>>(content, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return events;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading events: " + ex.Message);
                return null;
            }
        }


        private bool IsInternetAvailable()
        {
            var cm = (Android.Net.ConnectivityManager)GetSystemService(ConnectivityService);
            var network = cm?.ActiveNetworkInfo;
            return network != null && network.IsConnected;
        }

        private void ShowPushNotification(string title, string message)
        {
            var context = Application.Context;
            string channelId = "event_notify_channel";
            var manager = (NotificationManager)GetSystemService(NotificationService);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(channelId, "Оповіщення", NotificationImportance.Default)
                {
                    Description = "Нові події"
                };
                manager.CreateNotificationChannel(channel);
            }

            var builder = new Notification.Builder(context)
                .SetContentTitle(title)
                .SetContentText(message)
                .SetSmallIcon(Android.Resource.Drawable.IcDialogAlert)
                .SetAutoCancel(true);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                builder.SetChannelId(channelId);

            manager.Notify(2002, builder.Build());
        }
    }

    public class ScheduledEvent
    {
        public int Id { get; set; }
        public string DeviceName { get; set; }
        public string EventType { get; set; }
        public string Description { get; set; }
        public DateTime ScheduledTime { get; set; }
        public bool IsRecurring { get; set; }
        public TimeSpan? RecurrenceInterval { get; set; }
        public bool IsEnabled { get; set; }
        public string AudioFileName { get; set; }
        public string AudioFilePath { get; set; }
    }

}
#endif
