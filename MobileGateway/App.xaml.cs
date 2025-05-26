


#if ANDROID
using Android.Content;
using Android.OS;
#endif

namespace MobileGateway
{
    public partial class App : Application
    {
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            var token = Preferences.Get("UserToken", null);

            if (!string.IsNullOrEmpty(token))
            {
                MainPage = serviceProvider.GetRequiredService<MainPage>();
                StartForegroundServiceIfNeeded();
            }
            else
            {
                MainPage = serviceProvider.GetRequiredService<LoginPage>();
            }
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
    }

}
