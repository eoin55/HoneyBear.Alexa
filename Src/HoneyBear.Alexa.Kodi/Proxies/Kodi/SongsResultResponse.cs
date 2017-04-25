using System.Collections.ObjectModel;

namespace HoneyBear.Alexa.Kodi.Proxies.Kodi
{
    internal sealed class SongsResultResponse
    {
        public LimitResponse Limits { get; set; }
        public Collection<SongResponse> Songs { get; set; }
    }
}