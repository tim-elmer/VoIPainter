using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using VoIPainter.Model.Logging;

namespace VoIPainter.Control
{
    /// <summary>
    /// Handles serving images to phone
    /// </summary>
    public class ImageServerController
    {
        private CancellationTokenSource _cancellationTokenSource;
        private readonly LogController _logController;
        private readonly MainController _mainController;

        public ImageServerController(MainController mainController)
        {
            _mainController = mainController ?? throw new ArgumentNullException(nameof(mainController));

            _logController = _mainController.LogController;
        }

        public void Run()
        {
            if (!(_cancellationTokenSource is null))
                _cancellationTokenSource.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            _ = Task.Factory.StartNew(Worker, _cancellationTokenSource.Token);
        }

        private void Worker(object cancellationToken)
        {
            using var httpListener = new HttpListener();

            _logController.Log(new Entry(LogSeverity.Info, Strings.StatusImageServerStarting));

            var token = (CancellationToken)cancellationToken;

            httpListener.Prefixes.Clear();
            httpListener.Prefixes.Add($"http://{GetIp()}/");
            httpListener.Start();

            while (true)
            {
                if (token.IsCancellationRequested)
                    break;

                var context = httpListener.GetContext();
                var request = context.Request;
                var response = context.Response;

                if (token.IsCancellationRequested)
                    break;

                if (_mainController.ImageReformatController is null || _mainController.ImageReformatController.Image is null)
                {
                    _logController.Log(new Entry(LogSeverity.Error, string.Format(Strings.StatusPhoneRequestNotReady, request.RemoteEndPoint.Address)));
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.Close();
                    continue;
                }

                var thumbnail = request.Url.ToString().Equals($"http://{GetIp()}/bg-tn.png");

                if (thumbnail)
                    _logController.Log(new Entry(LogSeverity.Info, string.Format(Strings.StatusPhoneRequestThumbnail, request.RemoteEndPoint.Address)));
                else if (request.Url.ToString().Equals($"http://{GetIp()}/bg.png"))
                    _logController.Log(new Entry(LogSeverity.Info, string.Format(Strings.StatusPhoneRequestImage, request.RemoteEndPoint.Address)));
                else
                {
                    _logController.Log(new Entry(LogSeverity.Error, string.Format(Strings.StatusPhoneRequestInvalid, request.RemoteEndPoint.Address, request.Url.AbsolutePath)));
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Close();
                    continue;
                }

                response.StatusCode = (int)HttpStatusCode.OK;

                var outputStream = response.OutputStream;

                if (!token.IsCancellationRequested)
                    _mainController.ImageReformatController.Stream(outputStream, thumbnail);
                    
                outputStream.Close();
                response.Close();
            }

            httpListener.Stop();
            httpListener.Close();
            _logController.Log(new Entry(LogSeverity.Info, Strings.StatusImageServerStopped));
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
