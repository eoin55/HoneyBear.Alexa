using System;

namespace HoneyBear.Alexa.Kodi.Models
{
    internal sealed class Song : IComparable<Song>, IEquatable<Song>
    {
        public const string S3Bucket = "lambda-honeybear-alexa";
        public static readonly string BaseUrl = $"https://s3-eu-west-1.amazonaws.com/{S3Bucket}/";

        public Song(int songId, Guid songRef, string artist, string album, string title, int track, string filePath)
        {
            SongId = songId;
            SongRef = songRef;
            Artist = artist;
            Album = album;
            Title = title;
            Track = track;
            FilePath = filePath;
            Url = $"{BaseUrl}songs/mp3/{songRef}.mp3";
        }

        public int SongId { get; }
        public Guid SongRef { get; }
        public string Artist { get; }
        public string Album { get; }
        public string Title { get; }
        public int Track { get; }
        public string FilePath { get; }
        public string Url { get; }

        public override string ToString() => $"Title: {Title}. Artist: {Artist}. Album: {Album}.";

        public int CompareTo(Song other)
        {
            var albumComparison = string.Compare(Album, other.Album, StringComparison.OrdinalIgnoreCase);
            return albumComparison != 0 ? albumComparison : Track.CompareTo(other.Track);
        }

        public bool Equals(Song other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return SongId == other.SongId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as Song;
            return other != null && Equals(other);
        }

        public override int GetHashCode() => SongId;
    }
}