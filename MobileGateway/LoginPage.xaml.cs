﻿#if ANDROID
using Android.Content;
using Android.OS;
#endif

using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
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

        private readonly string ApiBase = string.Empty;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        public LoginPage(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            InitializeComponent();

            _configuration = configuration;
            _serviceProvider = serviceProvider;

            ApiBase = _configuration["Api:BaseUrl"];

            var username = _configuration["Auth:Username"];
            var password = _configuration["Auth:Password"];
            var credentials = $"{username}:{password}";
            var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encoded);
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

        private async void OnForgotPasswordTapped(object sender, EventArgs e)
        {
            var url = "https://ec2-51-21-255-211.eu-north-1.compute.amazonaws.com/Authorization/RecoverPassword";
            await Launcher.OpenAsync(url);
        }

        private async void OnRegisterTapped(object sender, EventArgs e)
        {
            var url = "https://ec2-51-21-255-211.eu-north-1.compute.amazonaws.com/Authorization/Register";
            await Launcher.OpenAsync(url);
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
            StartForegroundServiceIfNeeded();
            await DisplayAlert("Успіх", $"Вітаю, {user.Username}!", "OK");
            Application.Current!.MainPage = _serviceProvider.GetRequiredService<MainPage>();
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
