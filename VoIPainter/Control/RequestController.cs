using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using VoIPainter.Model.Logging;

namespace VoIPainter.Control
{
    /// <summary>
    /// Handle making requests to the phone
    /// </summary>
    public class RequestController
    {
        private HttpClient _httpClient;
        private readonly HttpClientHandler _httpClientHandler;
        private readonly LogController _logController;

        //public event EventHandler LoggingEvent;

        public RequestController(LogController logController)
        {
            _logController = logController ?? throw new ArgumentNullException(nameof(logController));

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
        /// <param name="target">Target phone IP</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        public async void Send(string target, string username, string password)
        {
            _httpClient = new HttpClient(_httpClientHandler)
            {
                BaseAddress = new Uri($"https://{target}")
            };

            // Tell the phone we want to do something
            var request = new HttpRequestMessage(HttpMethod.Post, "/CGI/Execute");

            // The command
            var keyValues = new List<KeyValuePair<string, string>>
            {
                // Set the background and thumbnail to images on our computer
                new KeyValuePair<string, string>("XML", $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><setBackground><background><image>http://{ImageServerController.GetIp()}/bg.png</image><icon>http://{ImageServerController.GetIp()}/bg-tn.png</icon></background></setBackground>")
            };

            // Properly escape the command
            request.Content = new FormUrlEncodedContent(keyValues);

            // Apply credentials
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}")));

            // Send command
            var response = await _httpClient.SendAsync(request);

            // Handle response
            var responseContent = await response.Content.ReadAsStringAsync();
            _logController.Log(new Entry(responseContent.Contains("success", StringComparison.OrdinalIgnoreCase) ? LogSeverity.Info : LogSeverity.Error, string.Format(Strings.TGotResponse, response.StatusCode, responseContent)));

            // Clear credentials
            _httpClient.DefaultRequestHeaders.Authorization = null;
            GC.Collect();
        }

    }
}
