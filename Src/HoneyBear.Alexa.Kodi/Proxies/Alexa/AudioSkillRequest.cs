using System;
using Slight.Alexa.Framework.Models.Requests;

namespace HoneyBear.Alexa.Kodi.Proxies.Alexa
{
    public class AudioSkillRequest
    {
        public string Version { get; set; }

        public Session Session { get; set; }

        public AudioRequestBundle Request { get; set; }

        public Type GetRequestType()
        {
            if (Request == null)
                throw new InvalidOperationException("Request is null.");
            return Request.GetRequestType();
        }
    }
}
