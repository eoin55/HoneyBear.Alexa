using System.Net.Http;
using System.Net.Http.Headers;
using HoneyBear.Alexa.Kodi.Queries;

namespace HoneyBear.Alexa.Kodi.Infrastructure
{
    internal sealed class KodiHttpClient : HttpClient
    {
        public KodiHttpClient()
        {
            var settings = new AppSettingReader();
            BaseAddress = settings.KodiBaseUrl;
            DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", settings.KodiAuthHeader);
        }
    }
}