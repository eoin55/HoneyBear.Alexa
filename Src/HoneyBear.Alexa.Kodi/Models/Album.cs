namespace HoneyBear.Alexa.Kodi.Models
{
    internal sealed class Album
    {
        public Album(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public override string ToString() => Name;
    }
}