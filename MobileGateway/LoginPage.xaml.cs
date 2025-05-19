#if ANDROID
using Android.Content;
using Android.OS;
#endif

using System.Text;
using System.Text.Json;

namespace MobileGateway
{
    public partial class LoginPage : ContentPage
    {
        private static readonly HttpClient _httpClient = new(
            new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            });

        private const string ApiBase = "https://ec2-51-21-255-211.eu-north-1.compute.amazonaws.com/server/api";
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public LoginPage()
        {
            InitializeComponent();
            StartForegroundServiceIfNeeded();
        }

        private void StartForegroundServiceIfNeeded()
        {
#if ANDROID
            var context = Android.App.Application.Context;
            var intent = new Intent(context, typeof(MobileGateway.EventForegroundService));

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                context.StartForegroundService(intent);
            else
                context.StartService(intent);
#endif
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            errorLabel.IsVisible = false;

            var email = usernameEntry.Text?.Trim();
            var password = passwordEntry.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Будь ласка, заповніть логін і пароль.");
                return;
            }

            var loginRequest = new { Email = email, Password = password };
            var response = await _httpClient.PostAsync($"{ApiBase}/User/authenticate", CreateJsonContent(loginRequest));

            if (!response.IsSuccessStatusCode)
            {
                ShowError(response.StatusCode switch
                {
                    System.Net.HttpStatusCode.Forbidden => "Пошта не підтверджена.",
                    System.Net.HttpStatusCode.Unauthorized => "Невірні логін або пароль.",
                    _ => "Помилка входу. Спробуйте пізніше."
                });
                return;
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<UserDto>(responseBody, JsonOptions);

            if (user == null)
            {
                ShowError("Неможливо прочитати відповідь сервера.");
                return;
            }

            Preferences.Set("UserToken", user.Id.ToString());
            Preferences.Set("Username", user.Username);

            await RegisterMobileDeviceAsync(user.Id);
            await DisplayAlert("Успіх", $"Вітаю, {user.Username}!", "OK");
            Application.Current!.MainPage = new MainPage();
        }

        private async Task RegisterMobileDeviceAsync(int userId)
        {
            try
            {
                var serialNumber = GetDeviceSerialNumber();
                var checkUrl = $"{ApiBase}/device/user/{userId}/serial/{serialNumber}";
                var checkResponse = await _httpClient.GetAsync(checkUrl);

                if (checkResponse.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine("Пристрій вже зареєстрований.");
                    return;
                }

                var deviceDto = new
                {
                    Name = DeviceInfo.Name,
                    Type = "Mobile",
                    SerialNumber = serialNumber,
                    ScheduledTime = DateTime.UtcNow,
                    UserId = userId
                };

                var createResponse = await _httpClient.PostAsync($"{ApiBase}/device/add", CreateJsonContent(deviceDto));

                if (!createResponse.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"Не вдалося зареєструвати пристрій. Статус: {createResponse.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Помилка реєстрації пристрою: {ex.Message}");
            }
        }

        private static StringContent CreateJsonContent<T>(T obj) =>
            new(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

        private static string GetDeviceSerialNumber() =>
            $"{DeviceInfo.Platform}-{DeviceInfo.Model}-{DeviceInfo.Manufacturer}";

        private void ShowError(string message)
        {
            errorLabel.Text = message;
            errorLabel.IsVisible = true;
        }

        public class UserDto
        {
            public int Id { get; set; }
            public string Username { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public bool IsAdmin { get; set; }
            public string? Token { get; set; }
        }
    }
}
