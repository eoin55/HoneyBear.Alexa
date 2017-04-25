using System;
using System.IO;
using System.Net.Http;
using System.Text;
using HoneyBear.Alexa.Kodi.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HoneyBear.Alexa.Kodi.Proxies.Kodi;

namespace HoneyBear.Alexa.Kodi.Queries
{
    internal sealed class FileReader
    {
        private readonly HttpClient _http;

        public FileReader()
        {
            _http = new KodiHttpClient();
        }

        public bool TryFetchFile(string filePath, Guid songRef, out Stream file)
        {
            file = null;

            var json = new
            {
                id = 0,
                jsonrpc = "2.0",
                method = "Files.PrepareDownload",
                _params = new
                {
                    path = filePath
                }
            };

            string url;
            try
            {
                var content = JObject.FromObject(json).ToString().Replace("_params", "params");
                var response = _http.PostAsync("jsonrpc", new StringContent(content, Encoding.UTF8, "application/json")).Result;
                if (!response.IsSuccessStatusCode)
                    return false;

                var raw = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<FileRootResponse>(raw);
                url = result.Result.Details.Path;
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to retrieve song ({filePath}) URL.", e);
            }

            try
            {
                file = _http.GetAsync(url).Result.Content.ReadAsStreamAsync().Result;
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to download {url}.", e);
            }

            return true;
        }
    }
}