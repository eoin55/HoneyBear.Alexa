namespace HoneyBear.Alexa.Kodi.Proxies.Alexa
{
    public class Error
    {
        public string Type { get; set; }

        public string Message { get; set; }

        public override string ToString() => $"Type={Type}|Message={Message}";
    }
}