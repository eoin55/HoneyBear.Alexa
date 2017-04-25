using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using HoneyBear.Alexa.Kodi.Models;

namespace HoneyBear.Alexa.Kodi.Queries
{
    internal sealed class SongQueueReader
    {
        private readonly AmazonDynamoDBClient _db;

        public SongQueueReader()
        {
            var config = new AmazonDynamoDBConfig {ServiceURL = "https://dynamodb.eu-west-1.amazonaws.com"};
            _db = new AmazonDynamoDBClient(config);
        }

        public IList<Song> QueuedSongs()
        {
            var songs = new List<Song>();
            try
            {
                var table = Table.LoadTable(_db, "SongQueue");
                var search = table.Scan(new ScanFilter());

                foreach (var item in search.GetRemainingAsync().Result)
                {
                    var songId = item["SongId"].AsInt();
                    var songRef = item["SongRef"].AsGuid();
                    var artist = item["Artist"].AsString();
                    var album = item["Album"].AsString();
                    var title = item["Title"].AsString();
                    var track = item["Track"].AsInt();
                    var filePath = item["FilePath"].AsString();
                    songs.Add(new Song(songId, songRef, artist, album, title, track, filePath));
                }
            }
            catch (Exception e)
            {
                throw new Exception("Failed to fetch songs from queue from DynamoDb.", e);
            }

            return songs.OrderBy(s => s).ToList();
        }
    }
}