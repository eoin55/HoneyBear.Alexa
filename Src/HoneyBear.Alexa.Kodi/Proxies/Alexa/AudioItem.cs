using Newtonsoft.Json;

namespace HoneyBear.Alexa.Kodi.Proxies.Alexa
{
    public class AudioItem
    {
        [JsonProperty("stream", NullValueHandling = NullValueHandling.Ignore)]
        public AudioStream Stream { get; set; }
    }
}
