using System;
using Slight.Alexa.Framework.Models.Requests.RequestTypes;

namespace HoneyBear.Alexa.Kodi.Proxies.Alexa
{
    public class AudioRequestBundle : RequestBundle
    {
        public new Type GetRequestType()
        {
            switch (Type)
            {
                case "IntentRequest":
                    return typeof(IIntentRequest);
                case "LaunchRequest":
                    return typeof(ILaunchRequest);
                case "SessionEndedRequest":
                    return typeof(ISessionEndedRequest);
                case "AudioPlayer.PlaybackStarted":
                    return typeof(IAudioPlayerPlaybackStartedRequest);
                case "AudioPlayer.PlaybackFinished":
                    return typeof(IAudioPlayerPlaybackFinishedRequest);
                case "AudioPlayer.PlaybackStopped":
                    return typeof(IAudioPlayerPlaybackStoppedRequest);
                case "AudioPlayer.PlaybackNearlyFinished":
                    return typeof(IAudioPlayerPlaybackNearlyFinishedRequest);
                case "AudioPlayer.PlaybackFailed":
                    return typeof(IAudioPlayerPlaybackFailedRequest);
                default:
                    return typeof(IUnknownRequest);
            }
        }

        public string Token { get; set; }

        public int OffsetInMilliseconds { get; set; }

        public Error Error { get; set; }

        public override string ToString() => $"Type={Type}|Reason={Reason}|(Error={Error})";
    }
}
