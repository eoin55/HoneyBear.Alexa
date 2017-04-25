using System;
using System.Collections.ObjectModel;

namespace HoneyBear.Alexa.Kodi.Proxies.Kodi
{
    internal sealed class SongResponse : IComparable<SongResponse>
    {
        public int SongId { get; set; }
        public string Album { get; set; }
        public Collection<string> Artist { get; set; }
        public string File { get; set; }
        public string Label { get; set; }
        public int Track { get; set; }

        public int CompareTo(SongResponse other)
        {
            var albumComparison = string.Compare(Album, other.Album, StringComparison.OrdinalIgnoreCase);
            return albumComparison != 0 ? albumComparison : Track.CompareTo(other.Track);
        }
    }
}