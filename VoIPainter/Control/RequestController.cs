using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using VoIPainter.Model.Logging;
using System.Threading.Tasks;
using System.Globalization;

namespace VoIPainter.Control
{
    /// <summary>
    /// Handle making requests to the phone
    /// </summary>
    public class RequestController : IDisposable
    {
        private HttpClient _httpClient;
        private readonly HttpClientHandler _httpClientHandler;
        private readonly MainController _mainController;
        private readonly LogController _logController;

        //public event EventHandler LoggingEvent;

        public RequestController(MainController mainController)
        {
            _mainController = mainController ?? throw new ArgumentNullException(nameof(mainController));
            _logController = _mainController.LogController;

            // Ignore bad certs
            _httpClientHandler = new HttpClientHandler()
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, certChain, policyErrors) => true
            };
        }

        /// <summary>
        /// Send request
        /// </summary>
        /// <param name="password">Password</param>
        public async Task Send(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException(nameof(password), Strings.ValidationPassword);

            _logController.Log(new Entry(LogSeverity.Info, Strings.StatusPhoneSendingCommand));

            _httpClient = new HttpClient(_httpClientHandler)
            {
                BaseAddress = new Uri($"https://{_mainController.SettingsController.LastTarget}")
            };

            // Tell the phone we want to do something
            using var request = new HttpRequestMessage(HttpMethod.Post, "/CGI/Execute");

            // The command
            var keyValues = new List<KeyValuePair<string, string>>
            {
                // Set the background and thumbnail to images on our computer
                new KeyValuePair<string, string>("XML", $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><setBackground><background><image>http://{ImageServerController.GetIp()}/bg.png</image><icon>http://{ImageServerController.GetIp()}/bg-tn.png</icon></background></setBackground>")
            };

            // Properly escape the command
            request.Content = new FormUrlEncodedContent(keyValues);

            // Apply credentials
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_mainController.SettingsController.LastUser}:{password}")));

            // Send command
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

            // Handle response
            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            _logController.Log(new Entry(responseContent.Contains("success", StringComparison.OrdinalIgnoreCase) ? LogSeverity.Info : LogSeverity.Error, string.Format(CultureInfo.InvariantCulture, Strings.TGotResponse, response.StatusCode, responseContent)));

            // Clear credentials
            _httpClient.DefaultRequestHeaders.Authorization = null;
            GC.Collect();
        }

        protected virtual void Dispose(bool disposeManaged)
        {
            if (disposeManaged)
            {
                if (!(_httpClient is null))
                    _httpClient.Dispose();
                if (!(_httpClientHandler is null))
                    _httpClientHandler.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
