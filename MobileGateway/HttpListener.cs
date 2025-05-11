using System;
using System.Net;
using System.Text; // Explicitly using System.Text.Encoding
using System.Threading.Tasks;

#if ANDROID
using Android.Media;
using Application = Android.App.Application;
#endif

namespace MobileGateway
{
    public class HttpListener
    {
        private System.Net.HttpListener _listener; // Fully qualified name to avoid ambiguity
        private bool _isRunning;

        public HttpListener()
        {
            _listener = new System.Net.HttpListener(); // Fully qualified name to avoid ambiguity
            _listener.Prefixes.Add("http://*:8080/"); // Приймаємо запити на порту 8080
        }

        public async Task Start()
        {
            _isRunning = true;
            _listener.Start();
            while (_isRunning)
            {
                var context = await _listener.GetContextAsync();
                var request = context.Request;
                var response = context.Response;

                // Перевіряємо запит на конкретну URL
                if (request.Url.AbsolutePath == "/play-sound")
                {
                    // Наприклад, програвання звуку
                    PlaySound();

                    // Відповідь
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes("Sound played successfully!"); // Fully qualified name
                    response.ContentLength64 = buffer.Length;
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }
                else
                {
                    // Якщо запит не знайдений
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes("Invalid request."); // Fully qualified name
                    response.ContentLength64 = buffer.Length;
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }

                response.OutputStream.Close();
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _listener.Stop();
        }

        private void PlaySound()
        {
#if ANDROID
            var context = Application.Context;
            try
            {
                var mediaPlayer = new MediaPlayer();
                var afd = context.Assets.OpenFd("sound.mp3"); // Помісти sound.mp3 в папку Resources
                mediaPlayer.SetDataSource(afd.FileDescriptor, afd.StartOffset, afd.Length);
                afd.Close();
                mediaPlayer.Prepare();
                mediaPlayer.Start();
            }
            catch (Exception ex)
            {
                // Логування помилки
                Console.WriteLine("Error playing sound: " + ex.Message);
            }
#endif
        }
    }
}
