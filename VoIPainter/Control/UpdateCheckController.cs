using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Globalization;

namespace VoIPainter.Control
{
    /// <summary>
    /// Handles checking for updates
    /// </summary>
    public class UpdateCheckController
    {
        private readonly LogController _logController;

        public UpdateCheckController(LogController logController) => _logController = logController ?? throw new ArgumentNullException(nameof(logController));

        public async Task<Model.UpdateResponse> GetUpdateAvailable()
        {
            _logController.Log(new Model.Logging.Entry(Model.Logging.LogSeverity.Info, Strings.StatusUpdateCheck));

            using var httpClient = new HttpClient()
            {
                BaseAddress = new Uri("https://api.github.com/repos/tim-elmer/VoIPainter/releases/latest")
            };

            // GitHub requires a user agent
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("tim-elmer/VoIPainter");

            // The request URI is in our base address, but we need to specify one to match the signature
            using var request = new HttpRequestMessage(HttpMethod.Get, "");
            var keyValues = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("accept", "application/vnd.github.v3+json"),
                new KeyValuePair<string, string>("owner", "tim-elmer"),
                new KeyValuePair<string, string>("repo", "VoIPainter")
            };
            request.Content = new FormUrlEncodedContent(keyValues);

            HttpResponseMessage response;
            try
            {
                response = await httpClient.SendAsync(request).ConfigureAwait(false);
                var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                var updateResponse = JsonSerializer.Deserialize<Model.UpdateResponse>(responseContent);

                _logController.Log(new Model.Logging.Entry(Model.Logging.LogSeverity.Info, Strings.StatusUpdateCheckDone));

                // Check for greater version
                if (System.Reflection.Assembly.GetExecutingAssembly().GetName().Version < updateResponse.Version)
                {
                    _logController.Log(new Model.Logging.Entry(Model.Logging.LogSeverity.Info, Strings.StatusUpdateCheckPositive));
                    return updateResponse;
                }
                else
                    _logController.Log(new Model.Logging.Entry(Model.Logging.LogSeverity.Info, Strings.StatusUpdateCheckNegative));
            }
            catch (Exception e)
            {
                _logController.Log(new Model.Logging.Entry(Model.Logging.LogSeverity.Warning, string.Format(CultureInfo.InvariantCulture, Strings.StatusUpdateCheckFail, e)));
            }

            return null;
        }
    }
}
