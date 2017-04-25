using Newtonsoft.Json;

namespace HoneyBear.Alexa.Kodi.Proxies.Alexa
{
    public class AudioStream
    {
        [JsonProperty("token", NullValueHandling = NullValueHandling.Ignore)]
        public string Token { get; set; }

        [JsonProperty("expectedPreviousToken", NullValueHandling = NullValueHandling.Ignore)]
        public string ExpectedPreviousToken { get; set; }

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }

        [JsonProperty("offsetInMilliseconds", NullValueHandling = NullValueHandling.Ignore)]
        public int OffsetInMilliseconds { get; set; }
    }
}
