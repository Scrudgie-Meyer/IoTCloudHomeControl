using System.Text;
using System.Text.Json;

namespace Server.Services
{
    public interface IEmailService
    {
        Task SendConfirmationEmail(string toEmail, string token);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;

        public EmailService(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task SendConfirmationEmail(string toEmail, string token)
        {
            var apiKey = Environment.GetEnvironmentVariable("BrevoApiKey");
            var frontendUrl = _config["AppSettings:FrontendUrl"];
            var confirmationLink = $"{frontendUrl}/Authorization/ConfirmEmail?token={token}";

            var payload = new
            {
                sender = new
                {
                    name = _config["Brevo:FromName"],
                    email = _config["Brevo:FromEmail"]
                },
                to = new[]
                {
                    new { email = toEmail }
                },
                subject = "Confirm your email for IOT Cloud Control",
                htmlContent = $"<p>Please confirm your email by clicking <a href=\"{confirmationLink}\">here</a>.</p>"
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("api-key", apiKey);
            _httpClient.DefaultRequestHeaders.Add("accept", "application/json");

            var response = await _httpClient.PostAsync("https://api.brevo.com/v3/smtp/email", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new System.Exception($"Failed to send email via Brevo: {error}");
            }
        }
    }
}
