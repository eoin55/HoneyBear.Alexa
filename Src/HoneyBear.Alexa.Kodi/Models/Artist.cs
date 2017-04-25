namespace HoneyBear.Alexa.Kodi.Models
{
    internal sealed class Artist
    {
        public Artist(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public override string ToString() => Name;
    }
}