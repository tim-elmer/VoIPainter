using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace VoIPainter.Model
{
    public class UpdateResponse
    {
        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; }

        [JsonPropertyName("tag_name")]
        public string TagName { get; set; }

        [JsonPropertyName("body")]
        public string Body { get; set; }

        [JsonPropertyName("published_at")]
        public DateTime PublishedAt { get; set; }
        public Version Version
        {
            get
            {
                var versionNumbers = TagName.Replace("v", "").Split('.').Select(e => int.Parse(e));
                return new Version(versionNumbers.ElementAt(0), versionNumbers.ElementAt(1), 0, versionNumbers.ElementAt(2));
            }
        }
    }
}
