using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using VoIPainter.Model.Logging;

namespace VoIPainter.Control
{
    /// <summary>
    /// Handles serving files to phone
    /// </summary>
    public class ServerController : IDisposable
    {
        private CancellationTokenSource _cancellationTokenSource;
        private readonly LogController _logController;
        private readonly MainController _mainController;

        public ServerController(MainController mainController)
        {
            _mainController = mainController ?? throw new ArgumentNullException(nameof(mainController));

            _logController = _mainController.LogController;
        }

        /// <summary>
        /// Run the server
        /// </summary>
        public void Run()
        {
            if (!(_cancellationTokenSource is null))
                _cancellationTokenSource.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            _ = Task.Run(() => Worker(_cancellationTokenSource.Token));
        }

        /// <summary>
        /// Worker thread
        /// </summary>
        /// <param name="cancellationToken">Token for canceling server operation</param>
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

                // Handle thumbnail
                if (request.Url.ToString().Equals($"http://{GetIp()}/bg-tn.png", StringComparison.InvariantCultureIgnoreCase))
                    ServeImage(request, response, token, true);

                // Handle image
                else if (request.Url.ToString().Equals($"http://{GetIp()}/bg.png", StringComparison.InvariantCultureIgnoreCase))
                    ServeImage(request, response, token);

                // Handle ringtone
                else if (request.Url.ToString().Equals($"http://{GetIp()}/rt.raw", StringComparison.InvariantCultureIgnoreCase))
                    ServeTone(request, response, token);

                // Handle invalid
                else
                {
                    _logController.Log(new Entry(LogSeverity.Error, string.Format(CultureInfo.InvariantCulture, Strings.StatusPhoneRequestInvalid, request.RemoteEndPoint.Address, request.Url.AbsolutePath)));
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Close();
                    continue;
                }
            }

            httpListener.Stop();
            httpListener.Close();
            _logController.Log(new Entry(LogSeverity.Info, Strings.StatusImageServerStopped));
        }

        /// <summary>
        /// Serve an image to the phone
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="response">The response</param>
        /// <param name="token">Cancelation token</param>
        /// <param name="thumbnail">Request is for thumbnail</param>
        private void ServeImage(HttpListenerRequest request, HttpListenerResponse response, CancellationToken token, bool thumbnail = false)
        {
            _logController.Log(new Entry(LogSeverity.Info, string.Format(CultureInfo.InvariantCulture, thumbnail ? Strings.StatusPhoneRequestThumbnail : Strings.StatusPhoneRequestImage, request.RemoteEndPoint.Address)));

            // Check for image availability
            if (_mainController.ImageReformatController is null || _mainController.ImageReformatController.Image is null)
            {
                _logController.Log(new Entry(LogSeverity.Error, string.Format(CultureInfo.InvariantCulture, Strings.StatusPhoneRequestNotReady, request.RemoteEndPoint.Address)));
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            else
            { 
                response.StatusCode = (int)HttpStatusCode.OK;

                if (!token.IsCancellationRequested)
                    _mainController.ImageReformatController.Stream(response.OutputStream, thumbnail);
                else
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.OutputStream.Close();
            }
            response.Close();
        }

        /// <summary>
        /// Serve a ringtone to the phone
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="response">The response</param>
        /// <param name="token">Cancelation token</param>
        private void ServeTone(HttpListenerRequest request, HttpListenerResponse response, CancellationToken token)
        {
            _logController.Log(new Entry(LogSeverity.Info, string.Format(CultureInfo.InvariantCulture, Strings.StatusPhoneRequestRingtone, request.RemoteEndPoint.Address)));

            // Check for ringtone availability
            if (_mainController.RingtoneReformatController is null || _mainController.RingtoneReformatController.Ringtone is null)
            {
                _logController.Log(new Entry(LogSeverity.Error, string.Format(CultureInfo.InvariantCulture, Strings.StatusPhoneRequestNotReady, request.RemoteEndPoint.Address)));
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.OK;

                if (!token.IsCancellationRequested)
                {
                    _mainController.RingtoneReformatController.Ringtone.Position = 0;

                    var buffer = new byte[_mainController.RingtoneReformatController.Ringtone.Length];
                    _mainController.RingtoneReformatController.Ringtone.Read(buffer);
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                }
                else
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.OutputStream.Close();
            }
            response.Close();
        }

        /// <summary>
        /// Stop the server
        /// </summary>
        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            _logController.Log(new Entry(LogSeverity.Info, Strings.StatusImageServerStopping));
        }

        /// <summary>
        /// Get this host's local IP
        /// </summary>
        /// <returns></returns>
        public static string GetIp()
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP);
            // Bounce off CloudFlare to get our IP. Our local IP. Yes, it's silly, but it works. We want a valid socket to grab the local endpoint from, and this gets us one.
            
            socket.Connect("1.1.1.1", 65530);
            var endPoint = socket.LocalEndPoint as IPEndPoint;
            return endPoint.Address.ToString();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposeManaged)
        {
            if (disposeManaged && !(_cancellationTokenSource is null))
                _cancellationTokenSource.Dispose();
        }
    }
}
