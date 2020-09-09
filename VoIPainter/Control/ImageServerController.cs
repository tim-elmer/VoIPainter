using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using VoIPainter.Model;
using VoIPainter.Model.Logging;

namespace VoIPainter.Control
{
    public class ImageServerController
    {
        private readonly HttpListener _httpListener = new HttpListener();
        private readonly ImageReformatController _imageReformatController;
        private string _image;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly LogController _logController;

        public string Image
        {
            get => _image;
            set
            {
                _image = value;
                _imageReformatController.Path = value;
            }
        }

        public Phone.ScreenInfo ScreenInfo { get; set; }

        public ImageServerController(LogController logController)
        {
            _logController = logController ?? throw new ArgumentNullException(nameof(logController));
            _imageReformatController = new ImageReformatController(_logController);
        }

        public void Run() => ThreadPool.QueueUserWorkItem(new WaitCallback(Worker), _cancellationTokenSource.Token);

        private void Worker(object cancellationToken)
        {
            var token = (CancellationToken)cancellationToken;

            if (string.IsNullOrWhiteSpace(Image))
            {
                _logController.Log(new Entry(LogSeverity.Error, Strings.ValidationImage));
                return;
            }

            _httpListener.Prefixes.Clear();
            _httpListener.Prefixes.Add($"http://{GetIp()}/");
            _httpListener.Start();

            while (!token.IsCancellationRequested)
            {
                _logController.Log(new Entry(LogSeverity.Info, Strings.StatusPhoneAwaiting));
                var context = _httpListener.GetContext();
                var request = context.Request;
                var response = context.Response;
                
                var thumbnail = request.Url.ToString().Equals($"http://{GetIp()}/bg-tn.png");

                if (thumbnail)
                    _logController.Log(new Entry(LogSeverity.Info, Strings.StatusPhoneRequestThumbnail));
                else if (request.Url.ToString().Equals($"http://{GetIp()}/bg.png"))
                    _logController.Log(new Entry(LogSeverity.Info, Strings.StatusPhoneRequestImage));
                else
                {
                    _logController.Log(new Entry(LogSeverity.Error, Strings.StatusPhoneRequestInvalid));
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
                }

                response.StatusCode = (int)HttpStatusCode.OK;

                var outputStream = response.OutputStream;
                _imageReformatController.Format(outputStream, ScreenInfo, thumbnail);
                outputStream.Close();
            }

            _httpListener.Stop();
            _httpListener.Close();
            _logController.Log(new Entry(LogSeverity.Info, Strings.StatusFinished));
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            _logController.Log(new Entry(LogSeverity.Info, Strings.StatusImageServerStopping));
        }

        public static string GetIp()
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP);
            // Bounce off Google to get our IP. Our local IP. Yes, it's silly, but it works. We want a valid socket to grab the local endpoint from, and this gets us one.
            socket.Connect("8.8.8.8", 65530);
            var endPoint = socket.LocalEndPoint as IPEndPoint;
            return endPoint.Address.ToString();
        }
    }
}
