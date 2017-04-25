using Newtonsoft.Json;
using Slight.Alexa.Framework.Models.Responses;

namespace HoneyBear.Alexa.Kodi.Proxies.Alexa
{
    public class AudioResponse : Response
    {
        [JsonProperty("directives", NullValueHandling = NullValueHandling.Ignore)]
        public Directive[] Directives { get; set; }
    }
}
