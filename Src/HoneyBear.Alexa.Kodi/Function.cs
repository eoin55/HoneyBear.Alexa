using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Lambda.Core;
using HoneyBear.Alexa.Kodi.Commands;
using HoneyBear.Alexa.Kodi.Models;
using HoneyBear.Alexa.Kodi.Queries;
using Slight.Alexa.Framework.Models.Requests.RequestTypes;
using Slight.Alexa.Framework.Models.Responses;
using JsonSerializer = Amazon.Lambda.Serialization.Json.JsonSerializer;
using HoneyBear.Alexa.Kodi.Proxies.Alexa;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(JsonSerializer))]

namespace HoneyBear.Alexa.Kodi
{
    public sealed class Function
    {
        private ILambdaLogger _log;

        private readonly SongQueueReader _queueReader;
        private readonly SongQueueWriter _queueWriter;
        private readonly LibraryWriter _library;
        private readonly SongReader _songs;

        private static Song _currentSong;
        private static int _currentOffsetInMilliseconds;

        public Function()
        {
            _queueReader = new SongQueueReader();
            _queueWriter = new SongQueueWriter();
            _library = new LibraryWriter();
            _songs = new SongReader();
        }

        public AudioSkillResponse Handle(AudioSkillRequest input, ILambdaContext context)
        {
            _log = context.Logger;
            _log.LogLine($"Request type={input.GetRequestType()}.");

            if (input.GetRequestType() == typeof(ILaunchRequest))
                return HandleLaunchRequest();
            if (input.GetRequestType() == typeof(IIntentRequest))
                return HandleIntentRequest(input.Request);
            if (input.GetRequestType() == typeof(IAudioPlayerPlaybackNearlyFinishedRequest))
                return HandlePlaybackNearlyFinishedRequest(input.Request);
            if (input.GetRequestType() == typeof(IAudioPlayerPlaybackStartedRequest))
                return HandlePlaybackStartedRequest(input.Request);
            if (input.GetRequestType() == typeof(IAudioPlayerPlaybackStoppedRequest))
                return HandlePlaybackStoppedRequest(input.Request);
            if (input.GetRequestType() == typeof(IAudioPlayerPlaybackFailedRequest))
            {
                _log.LogLine($"Request: {input.Request}");
                return Failed("Failed to play song.");
            }
            return DefaultResponse(input.Request);
        }

        private AudioSkillResponse HandleLaunchRequest()
        {
            _log.LogLine("Default LaunchRequest made");

            return new AudioSkillResponse
            {
                Response = new AudioResponse
                {
                    ShouldEndSession = false,
                    OutputSpeech = new PlainTextOutputSpeech {Text = "Please select music to play."}
                },
                Version = "1.0"
            };
        }

        private AudioSkillResponse HandleIntentRequest(IIntentRequest request)
        {
            _log.LogLine($"Intent Requested {request.Intent.Name}");

            switch (request.Intent.Name)
            {
                case "RequestPlayAlbumByArtistIntent":
                case "RequestPlayAlbumIntent":
                case "RequestPlayArtistIntent":
                case "RequestPlaySongByArtistIntent":
                case "RequestPlaySongOnAlbumIntent":
                case "RequestPlaySongIntent":
                    return HandlePlayMusicRequest(request);
                case "AMAZON.PauseIntent":
                    return StopPlaying();
                case "AMAZON.CancelIntent":
                    return StopPlaying();
                case "AMAZON.ResumeIntent":
                    return HandleResumeRequest();
                case "AMAZON.NextIntent":
                    return HandleNextRequest();
                case "AMAZON.PreviousIntent":
                    return HandlePreviousRequest();
                default:
                    throw new Exception($"Failed to handle request {request.Intent.Name}");
            }
        }

        private AudioSkillResponse HandlePlayMusicRequest(IIntentRequest request)
        {
            var slots = request.Intent.Slots;
            var intent = request.Intent.Name;

            var requestedArtist = slots.ContainsKey("requestedArtist") ? slots["requestedArtist"].Value : null;
            var requestedAlbum = slots.ContainsKey("requestedAlbum") ? slots["requestedAlbum"].Value : null;
            var requestedSong = slots.ContainsKey("requestedSong") ? slots["requestedSong"].Value : null;

            var hasArtist = !string.IsNullOrEmpty(requestedArtist);
            var hasAlbum = !string.IsNullOrEmpty(requestedAlbum);
            var hasSong = !string.IsNullOrEmpty(requestedSong);

            var artistIsRequired = intent.Contains("Artist");
            var albumIsRequired = intent.Contains("Album");
            var songIsRequired = intent.Contains("Song");

            if (artistIsRequired && !hasArtist)
                return Failed("Artist input was empty");
            if (albumIsRequired && !hasAlbum)
                return Failed("Album input was empty");
            if (songIsRequired && !hasSong)
                return Failed("Song input was empty");

            var artist = new Artist(requestedArtist);
            var album = new Album(requestedAlbum);

            switch (intent)
            {
                case "RequestPlayAlbumByArtistIntent":
                    return PlayMusic(artist, album);
                case "RequestPlayAlbumIntent":
                    return PlayMusic(album);
                case "RequestPlayArtistIntent":
                    return PlayMusic(artist);
                case "RequestPlaySongByArtistIntent":
                    return PlayMusic(artist, requestedSong);
                case "RequestPlaySongOnAlbumIntent":
                    return PlayMusic(album, requestedSong);
                case "RequestPlaySongIntent":
                    return PlayMusic(requestedSong);
                default:
                    throw new Exception($"Failed to handle request {intent}");
            }
        }

        private AudioSkillResponse PlayMusic(Artist artist, Album album)
        {
            _log.LogLine($"Searching for songs by artist {artist} on album {album}...");
            var songs = _songs.FindFor(artist, album);
            return
                songs.Any()
                    ? PlaySong(songs.First(), songs, true)
                    : Failed($"Failed to find any songs for artist {artist} on album {album}");
        }

        private AudioSkillResponse PlayMusic(Artist artist, string requestedSong)
        {
            _log.LogLine($"Searching for song {requestedSong} by artist {artist}...");
            return
                _songs.TryFind(requestedSong, artist, out Song song)
                    ? PlaySong(song)
                    : Failed($"Failed to find song {requestedSong} by artist {artist}.");
        }

        private AudioSkillResponse PlayMusic(Album album, string requestedSong)
        {
            _log.LogLine($"Searching for song {requestedSong} on album {album}...");
            return
                _songs.TryFind(requestedSong, album, out Song song)
                    ? PlaySong(song)
                    : Failed($"Failed to find song {requestedSong} on album {album}.");
        }

        private AudioSkillResponse PlayMusic(Artist artist)
        {
            _log.LogLine($"Searching for songs by artist {artist}...");
            var songs = _songs.FindFor(artist);
            return
                songs.Any()
                    ? PlaySong(songs.First(), songs, true)
                    : Failed($"Failed to find any songs by artist {artist}");
        }

        private AudioSkillResponse PlayMusic(Album album)
        {
            _log.LogLine($"Searching for songs on album {album}...");
            var songs = _songs.FindFor(album);
            return
                songs.Any()
                    ? PlaySong(songs.First(), songs, true)
                    : Failed($"Failed to find any songs on album {album}");
        }

        private AudioSkillResponse PlayMusic(string requestedSong)
        {
            _log.LogLine($"Searching for song {requestedSong}...");
            return
                _songs.TryFind(requestedSong, out Song song)
                    ? PlaySong(song)
                    : Failed($"Failed to find song {requestedSong}.");
        }

        private AudioSkillResponse HandlePlaybackNearlyFinishedRequest(AudioRequestBundle request)
        {
            _log.LogLine($"Handle PlaybackNearlyFinished request (token={request.Token}).");

            Guid.TryParse(request.Token, out Guid songRef);
            var songs = _queueReader.QueuedSongs();
            var current = songs.FirstOrDefault(s => s.SongRef == songRef);
            if (current == null)
                return Failed($"Failed to find current song for token={songRef}");

            var index = songs.IndexOf(current);
            if (index + 1 < songs.Count)
            {
                var next = songs[index + 1];
                return PlaySong(next, songs, false);
            }

            _log.LogLine("No songs left in queue.");
            return DefaultResponse(request);
        }

        private AudioSkillResponse HandlePlaybackStartedRequest(AudioRequestBundle request)
        {
            _log.LogLine($"Handle PlaybackStarted request (token={request.Token}).");

            Guid.TryParse(request.Token, out Guid songRef);
            var songs = _queueReader.QueuedSongs();
            var current = songs.FirstOrDefault(s => s.SongRef == songRef);
            if (current == null)
                return Failed($"Failed to find current song for token={songRef}");

            _currentSong = current;
            return DefaultResponse(request);
        }

        private AudioSkillResponse HandlePlaybackStoppedRequest(AudioRequestBundle request)
        {
            _log.LogLine($"Handle PlaybackStopped request (token={request.Token} | offset={request.OffsetInMilliseconds}).");

            _currentOffsetInMilliseconds = request.OffsetInMilliseconds;
            return DefaultResponse(request);
        }

        private AudioSkillResponse HandleResumeRequest()
        {
            _log.LogLine($"Handle AMAZON.ResumeIntent (current song={_currentSong}).");

            if (_currentSong == null)
                return Failed("Resume failed; no current song stored in session.");

            var songs = _queueReader.QueuedSongs();
            var index = songs.IndexOf(_currentSong);
            return PlaySong(_currentSong, true, songs.Count - index - 1, _currentOffsetInMilliseconds);
        }

        private AudioSkillResponse HandleNextRequest()
        {
            _log.LogLine($"Handle AMAZON.NextIntent (current song={_currentSong}).");

            if (_currentSong == null)
                return Failed("Next failed; no current song stored in session.");

            var songs = _queueReader.QueuedSongs();
            var index = songs.IndexOf(_currentSong);
            return
                index + 1 >= songs.Count
                    ? Failed("Next failed; no more songs in queue.")
                    : PlaySong(songs[index + 1], true, songs.Count - index);
        }

        private AudioSkillResponse HandlePreviousRequest()
        {
            _log.LogLine($"Handle AMAZON.PreviousIntent (current song={_currentSong}).");

            if (_currentSong == null)
                return Failed("Previous failed; no current song stored in session.");

            var songs = _queueReader.QueuedSongs();
            var index = songs.IndexOf(_currentSong);
            return
                index == 0
                    ? Failed("Previous failed; no previous songs in queue.")
                    : PlaySong(songs[index - 1], true, songs.Count - index + 1);
        }

        private AudioSkillResponse PlaySong(Song next)
        {
            _queueWriter.Save(new[] {next});
            return PlaySong(next, true, 0);
        }

        private AudioSkillResponse PlaySong(Song next, IList<Song> songs, bool firstSong)
        {
            var index = songs.IndexOf(next);
            var remaining = songs.Count - index - 1;

            if (firstSong)
            {
                _log.LogLine($"Adding {songs.Count} songs to the queue.");
                _queueWriter.Save(songs);
            }

            return PlaySong(next, firstSong, remaining);
        }

        private AudioSkillResponse PlaySong(Song next, bool firstSong, int remaining) =>
            PlaySong(next, firstSong, remaining, 0);

        private AudioSkillResponse PlaySong(Song next, bool firstSong, int remaining, int offset)
        {
            _log.LogLine($"Play song: {next} | Remaining: {remaining} | URL: {next.Url}");

            _library.Save(next);

            var behavior = "ENQUEUE";
            PlainTextOutputSpeech speech = null;
            var token = next.SongRef.ToString();
            var previousToken = _currentSong?.SongRef.ToString();
            if (firstSong)
            {
                speech = new PlainTextOutputSpeech
                {
                    Text = $"Playing song: {next}. {remaining} song{(remaining == 1 ? "" : "s")} remaining in the queue."
                };
                behavior = "REPLACE_ALL";
                previousToken = null;
            }
            
            return new AudioSkillResponse
            {
                Response = new AudioResponse
                {
                    ShouldEndSession = true,
                    OutputSpeech = speech,
                    Directives = new[]
                    {
                        new Directive
                        {
                            Type = "AudioPlayer.Play",
                            PlayBehavior = behavior,
                            AudioItem = new AudioItem
                            {
                                Stream = new AudioStream
                                {
                                    Token = token,
                                    ExpectedPreviousToken = previousToken,
                                    Url = next.Url,
                                    OffsetInMilliseconds = offset
                                }
                            }
                        }
                    }
                },
                Version = "1.0"
            };
        }

        private AudioSkillResponse StopPlaying()
        {
            _log.LogLine("Stopping the player.");

            return new AudioSkillResponse
            {
                Response = new AudioResponse
                {
                    ShouldEndSession = true,
                    Directives = new[] {new Directive {Type = "AudioPlayer.Stop"}}
                },
                Version = "1.0"
            };
        }

        private AudioSkillResponse Failed(string message)
        {
            _log.LogLine(message);

            return new AudioSkillResponse
            {
                Response = new AudioResponse
                {
                    ShouldEndSession = false,
                    OutputSpeech = new PlainTextOutputSpeech {Text = $"{message}  Please try again."}
                },
                Version = "1.0"
            };
        }

        private AudioSkillResponse DefaultResponse(IRequest request)
        {
            _log.LogLine($"Request: {request}");

            return new AudioSkillResponse
            {
                Response = new AudioResponse {ShouldEndSession = true},
                Version = "1.0"
            };
        }
    }
}
