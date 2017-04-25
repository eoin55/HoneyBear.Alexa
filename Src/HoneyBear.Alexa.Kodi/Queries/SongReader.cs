using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using HoneyBear.Alexa.Kodi.Models;
using HoneyBear.Alexa.Kodi.Proxies.Kodi;

namespace HoneyBear.Alexa.Kodi.Queries
{
    internal sealed class SongReader
    {
        private const int PageSize = 20000;

        private readonly CacheReader _cache;
        private readonly KodiReader _kodi;
        private readonly LibraryReader _library;
        private readonly DateTime _now;

        public SongReader()
        {
            _cache = new CacheReader();
            _kodi = new KodiReader();
            _library = new LibraryReader();
            _now = DateTime.UtcNow;
        }

        public bool TryFind(string name, out Song song) => TryFind(name, 0, PageSize, out song, out int _);

        public bool TryFind(string name, Artist artist, out Song song)
        {
            var all = FetchSongs(0, PageSize, out int _).Result.Songs;
            var songs = all.Where(s => s.Artist.First().Equals(artist.Name, StringComparison.OrdinalIgnoreCase));
            var match = songs.FirstOrDefault(s => s.Label.StartsWith(name, StringComparison.OrdinalIgnoreCase));
            if (match == null)
            {
                song = null;
                return false;
            }
            song = FetchSong(match);
            return true;
        }

        public bool TryFind(string name, Album album, out Song song)
        {
            var all = FetchSongs(0, PageSize, out int _).Result.Songs;
            var songs = all.Where(s => s.Album.Equals(album.Name, StringComparison.OrdinalIgnoreCase));
            var match = songs.FirstOrDefault(s => s.Label.StartsWith(name, StringComparison.OrdinalIgnoreCase));
            if (match == null)
            {
                song = null;
                return false;
            }
            song = FetchSong(match);
            return true;
        }

        public IList<Song> FindFor(Artist artist)
        {
            var songs = FetchSongs(0, PageSize, out int _);
            var matches =
                songs.Result.Songs
                    .Where(s => s.Artist.First().Equals(artist.Name, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(s => s);
            return matches.Select(FetchSong).ToList();
        }

        public IList<Song> FindFor(Artist artist, Album album)
        {
            var songs = FetchSongs(0, PageSize, out int _).Result.Songs;
            var matches =
                songs
                    .Where(s =>
                        s.Artist.First().Equals(artist.Name, StringComparison.OrdinalIgnoreCase)
                        && s.Album.StartsWith(album.Name, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(s => s);
            return matches.Select(FetchSong).ToList();
        }

        public IList<Song> FindFor(Album album)
        {
            var songs = FetchSongs(0, PageSize, out int _).Result.Songs;
            var matches =
                songs
                    .Where(s => s.Album.Equals(album.Name, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(s => s);
            return matches.Select(FetchSong).ToList();
        }

        private bool TryFind(string name, int startIndex, int endIndex, out Song song, out int total)
        {
            var songs = FetchSongs(startIndex, endIndex, out total).Result.Songs;
            var match = songs.FirstOrDefault(s => s.Label.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (match == null)
            {
                match = songs.FirstOrDefault(s => s.Label.StartsWith(name, StringComparison.OrdinalIgnoreCase));
                if (match == null)
                {
                    song = null;
                    return false;
                }
            }
            song = FetchSong(match);
            return true;
        }

        private SongsRootResponse FetchSongs(int startIndex, int endIndex, out int total)
        {
            var key = Key(startIndex, endIndex);
            if (!_cache.TryGet(key, out string json))
                json = _kodi.FetchSongs(startIndex, endIndex, key);

            var result = JsonConvert.DeserializeObject<SongsRootResponse>(json);
            total = result.Result.Limits.Total;
            return result;
        }

        private Song FetchSong(SongResponse s) =>
            _library.TryGetSong(s.SongId, out Song song)
                ? song
                : new Song(s.SongId, Guid.NewGuid(), s.Artist.FirstOrDefault(), s.Album, s.Label, s.Track, s.File);

        private string Key(int startIndex, int endIndex) => $"songs/{_now:yyyy-MM-dd}/{startIndex}-{endIndex}.json";
    }
}