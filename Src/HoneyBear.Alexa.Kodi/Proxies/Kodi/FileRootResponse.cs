namespace HoneyBear.Alexa.Kodi.Proxies.Kodi
{
    internal sealed class FileRootResponse
    {
        public string Id { get; set; }
        public string JsonRpc { get; set; }
        public FileResultResponse Result { get; set; }
    }
}