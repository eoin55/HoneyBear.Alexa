using System;
using System.Net.Http;
using System.Text;
using HoneyBear.Alexa.Kodi.Commands;
using HoneyBear.Alexa.Kodi.Infrastructure;
using Newtonsoft.Json.Linq;

namespace HoneyBear.Alexa.Kodi.Queries
{
    internal sealed class KodiReader
    {
        private readonly HttpClient _http;
        private readonly CacheWriter _cache;

        public KodiReader()
        {
            _http = new KodiHttpClient();
            _cache = new CacheWriter();
        }

        public string FetchSongs(int startIndex, int endIndex, string cacheKey)
        {
            var request = new
            {
                Id = "libSongs",
                Jsonrpc = "2.0",
                Method = "AudioLibrary.GetSongs",
                Params = new
                {
                    Limits = new {Start = startIndex, End = endIndex},
                    Properties = new[] {"album", "artist", "file", "track"}
                },
                Sort = new {Order = "ascending", Method = "track", IgnoreArticle = true}
            };

            return Fetch(cacheKey, request);
        }

        public string FetchArtists(int startIndex, int endIndex, string cacheKey)
        {
            var request = new
            {
                Id = "libArtists",
                Jsonrpc = "2.0",
                Method = "AudioLibrary.GetArtists",
                Params = new
                {
                    Limits = new {Start = startIndex, End = endIndex},
                    Properties = new string[] {}
                },
                Sort = new {Order = "ascending", Method = "track", IgnoreArticle = true}
            };

            return Fetch(cacheKey, request);
        }

        private string Fetch(string cacheKey, object request)
        {
            string raw;
            try
            {
                var content = JObject.FromObject(request).ToString().ToLower();
                var response = _http.PostAsync("jsonrpc", new StringContent(content, Encoding.UTF8, "application/json")).Result;
                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Failed to retrieve songs from Kodi. Response code: {response.StatusCode}.");

                raw = response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception e)
            {
                throw new Exception("Failed to retrieve songs from Kodi", e);
            }

            _cache.Save(raw, cacheKey);

            return raw;
        }
    }
}