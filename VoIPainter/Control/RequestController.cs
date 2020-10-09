using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using VoIPainter.Model.Logging;
using System.Threading.Tasks;
using System.Globalization;
using System.Xml.Linq;

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

        /// <summary>
        /// Map for phone error descriptions
        /// </summary>
        private static readonly Dictionary<int, string> _phoneErrorDescriptions = new Dictionary<int, string>()
        {
            { 1, Strings.PhoneErrorParsingRequest },
            { 2, Strings.PhoneErrorFramingResponse },
            { 3, Strings.PhoneErrorInternalFile },
            { 4, Strings.PhoneErrorAuthentication }
        };

        public RequestController(MainController mainController)
        {
            _mainController = mainController ?? throw new ArgumentNullException(nameof(mainController));
            _logController = _mainController.LogController;

            // Ignore bad certs(!)
            // This is unfortunately necessary, as the chances of an IP phone having a valid cert are fairly low. We use HTTPS anyways though, because it still provides better security than sending our credential across the network in plaintext.
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
        public async Task Send(string password, Model.RequestMode requestMode)
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
            var keyValues = new List<KeyValuePair<string, string>>();

            switch (requestMode)
            {
                case Model.RequestMode.Background:
                    keyValues.Add(new KeyValuePair<string, string>("XML", $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><setBackground><background><image>http://{ServerController.GetIp()}/bg.png</image><icon>http://{ServerController.GetIp()}/bg-tn.png</icon></background></setBackground>"));
                    break;
                case Model.RequestMode.Ringtone:
                    keyValues.Add(new KeyValuePair<string, string>("XML", $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><setRingTone><ringTone>http://{ServerController.GetIp()}/rt.raw</ringTone></setRingTone>"));
                    break;
            }

            // Properly escape the command
            request.Content = new FormUrlEncodedContent(keyValues);

            // Apply credentials
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_mainController.SettingsController.LastUser}:{password}")));

            // Send command
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

            // Handle response
            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (responseContent.Contains("success", StringComparison.OrdinalIgnoreCase))
                _logController.Log(new Entry(LogSeverity.Info, Strings.StatusSuccess));
            else
            {
                var phoneError = ParsePhoneError(XElement.Parse(responseContent));
                if (string.IsNullOrWhiteSpace(phoneError.Message))
                    _logController.Log(new Entry(LogSeverity.Error, string.Format(CultureInfo.InvariantCulture, Strings.TRequestFailed, phoneError.Description)));
                else
                    _logController.Log(new Entry(LogSeverity.Error, string.Format(CultureInfo.InvariantCulture, Strings.TRequestFailedMessage, phoneError.Description, phoneError.Message)));
                    
            }

            // Clear credentials
            _httpClient.DefaultRequestHeaders.Authorization = null;
            GC.Collect();
        }

        /// <summary>
        /// Represents a CiscoIPPhoneError
        /// </summary>
        private struct PhoneError
        {
            /// <summary>
            /// The error number
            /// </summary>
            public int ErrorNumber { get; }

            /// <summary>
            /// The error description
            /// </summary>
            public string Description { get; }

            /// <summary>
            /// The error message
            /// </summary>
            public string Message { get; }

            public PhoneError(int errorNumber, string message, string detail)
            {
                ErrorNumber = errorNumber;
                Description = message;
                Message = detail;
            }
        }

        /// <summary>
        /// Parse a CiscoIPPhoneError
        /// </summary>
        /// <param name="responseContent">The raw response from the phone</param>
        /// <returns>The parsed PhoneError</returns>
        private static PhoneError ParsePhoneError(XElement responseContent)
        {
            if (!(responseContent.Element("type") is null))
                return new PhoneError(0, "", responseContent.Element("data").Value);
            else
            {
                // Expects `<CiscoIPPhoneError Number="x"/> optional error message <CiscoIPPhoneError>`
                var errorNumber = int.Parse(responseContent.Attribute("Number").Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
                return new PhoneError(errorNumber, _phoneErrorDescriptions[errorNumber], responseContent.Value);
            }
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
