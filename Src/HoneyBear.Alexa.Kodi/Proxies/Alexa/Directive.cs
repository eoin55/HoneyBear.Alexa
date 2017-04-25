using Newtonsoft.Json;

namespace HoneyBear.Alexa.Kodi.Proxies.Alexa
{
    public class Directive
    {
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("playBehavior", NullValueHandling = NullValueHandling.Ignore)]
        public string PlayBehavior { get; set; }

        [JsonProperty("audioItem", NullValueHandling = NullValueHandling.Ignore)]
        public AudioItem AudioItem { get; set; }
    }
}
