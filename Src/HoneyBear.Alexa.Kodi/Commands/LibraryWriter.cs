using System;
using System.Collections.Generic;
using System.IO;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.S3;
using Amazon.S3.Model;
using HoneyBear.Alexa.Kodi.Models;
using HoneyBear.Alexa.Kodi.Queries;

namespace HoneyBear.Alexa.Kodi.Commands
{
    internal sealed class LibraryWriter
    {
        private readonly AmazonS3Client _s3;
        private readonly AmazonDynamoDBClient _db;
        private readonly FileReader _file;
        private readonly LibraryReader _library;

        public LibraryWriter()
        {
            var s3Config = new AmazonS3Config { ServiceURL = "https://s3-eu-west-1.amazonaws.com" };
            var dbConfig = new AmazonDynamoDBConfig { ServiceURL = "https://dynamodb.eu-west-1.amazonaws.com" };

            _s3 = new AmazonS3Client(s3Config);
            _db = new AmazonDynamoDBClient(dbConfig);

            _file = new FileReader();
            _library = new LibraryReader();
        }

        public void Save(Song song)
        {
            if (_library.TryGetSong(song.SongId, out _))
                return;

            var file = FetchFileFromKodi(song);
            SaveToS3(song, file);
            SaveToDb(song);
        }

        private Stream FetchFileFromKodi(Song song)
        {
            if (!_file.TryFetchFile(song.FilePath, song.SongRef, out Stream file))
                throw new Exception($"Failed to find file for song ({song}).");
            return file;
        }

        private void SaveToS3(Song song, Stream file)
        {
            var key = song.Url.Replace(Song.BaseUrl, string.Empty);
            try
            {
                var request = new PutObjectRequest
                {
                    BucketName = Song.S3Bucket,
                    Key = key,
                    ContentType = "audio/mp3",
                    InputStream = file,
                    CannedACL = S3CannedACL.PublicRead
                };
                var response = _s3.PutObjectAsync(request).Result;
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to upload {key} to S3.", e);
            }
        }

        private void SaveToDb(Song song)
        {
            try
            {
                var items = new Dictionary<string, AttributeValue>
                {
                    {"SongId", new AttributeValue {N = song.SongId.ToString()}},
                    {"SongRef", new AttributeValue {S = song.SongRef.ToString()}},
                    {"Artist", new AttributeValue {S = song.Artist}},
                    {"Album", new AttributeValue {S = song.Album}},
                    {"Title", new AttributeValue {S = song.Title}},
                    {"Track", new AttributeValue {N = song.Track.ToString()}},
                    {"FilePath", new AttributeValue {S = song.FilePath}}
                };
                var response = _db.PutItemAsync(new PutItemRequest("Songs", items)).Result;
            }
            catch(Exception e)
            {
                throw new Exception($"Failed to save song ({song}) to DynamoDb.", e);
            }
        }
    }
}