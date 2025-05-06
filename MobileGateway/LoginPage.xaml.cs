using System.Text;
using System.Text.Json;

namespace MobileGateway;

public partial class LoginPage : ContentPage
{
    // HttpClient � ���������� �������� �����������
    private static readonly HttpClient _httpClient = new(
        new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        });

    private const string ApiUrl = "https://ec2-51-21-255-211.eu-north-1.compute.amazonaws.com/server/api/User/authenticate";

    public LoginPage()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        errorLabel.IsVisible = false;

        var email = usernameEntry.Text?.Trim();
        var password = passwordEntry.Text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            errorLabel.Text = "�������� ���� � ������.";
            errorLabel.IsVisible = true;
            return;
        }

        var loginRequest = new
        {
            Email = email,
            Password = password
        };

        var content = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json");

        try
        {
            var response = await _httpClient.PostAsync(ApiUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                string message = response.StatusCode switch
                {
                    System.Net.HttpStatusCode.Forbidden => "����� �� �����������.",
                    System.Net.HttpStatusCode.Unauthorized => "����� ���.",
                    _ => "������� �����."
                };

                errorLabel.Text = message;
                errorLabel.IsVisible = true;
                return;
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<UserDto>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (user == null)
            {
                errorLabel.Text = "��������� ������� �������.";
                errorLabel.IsVisible = true;
                return;
            }

            // TODO: �������� �����, ���� �������
            await DisplayAlert("����", $"³���, {user.Username}!", "OK");

            // �������� �� ������� �������
            Application.Current.MainPage = new MainPage();
        }
        catch (Exception ex)
        {
            errorLabel.Text = "������� �'������� � ��������.";
            errorLabel.IsVisible = true;
        }
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
