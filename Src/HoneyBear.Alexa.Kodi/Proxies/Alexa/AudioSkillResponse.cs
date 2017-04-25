using Newtonsoft.Json;
using Slight.Alexa.Framework.Models.Responses;

namespace HoneyBear.Alexa.Kodi.Proxies.Alexa
{
    public class AudioSkillResponse : SkillResponse
    {
        [JsonProperty("response", NullValueHandling = NullValueHandling.Ignore)]
        public new AudioResponse Response { get; set; }
    }
}
