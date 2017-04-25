namespace HoneyBear.Alexa.Kodi.Proxies.Kodi
{
    internal sealed class SongsRootResponse
    {
        public string Id { get; set; }
        public string JsonRpc { get; set; }
        public SongsResultResponse Result { get; set; }
    }
}