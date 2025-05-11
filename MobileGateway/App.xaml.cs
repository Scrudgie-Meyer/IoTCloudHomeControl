
namespace MobileGateway
{
    public partial class App : Application
    {
        private HttpListener _httpServer;

        public App()
        {
            InitializeComponent();

            _httpServer = new HttpListener();
            StartServer();

            string? token = Preferences.Get("UserToken", null);

            if (!string.IsNullOrEmpty(token))
            {
                MainPage = new MainPage();
            }
            else
            {
                MainPage = new LoginPage(); 
            }
        }

        private async void StartServer()
        {
            await Task.Run(() => _httpServer.Start());
        }

  
        protected override void OnResume()
        {
            base.OnResume();
            StartServer();
        }
    }
}

