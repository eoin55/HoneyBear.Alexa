using System;
using System.IO;
using HoneyBear.Alexa.Kodi.Proxies.Config;
using Newtonsoft.Json;

namespace HoneyBear.Alexa.Kodi.Queries
{
    internal sealed class AppSettingReader
    {
#if DEBUG
        private const string FileName = "app.settings.development.json";
#else
        private const string FileName = "app.settings.production.json";
#endif
        private readonly AppSettingsRoot _settings;

        public AppSettingReader()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), FileName);
            try
            {
                var json = File.ReadAllText(filePath);
                _settings = JsonConvert.DeserializeObject<AppSettingsRoot>(json);
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to read app settings from file ({filePath}).", e);
            }
        }

        public Uri KodiBaseUrl => new Uri(_settings.AppSettings.KodiBaseUrl);

        public string KodiAuthHeader => _settings.AppSettings.KodiAuthHeader;
    }
}