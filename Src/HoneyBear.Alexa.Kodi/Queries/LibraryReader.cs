using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using HoneyBear.Alexa.Kodi.Models;

namespace HoneyBear.Alexa.Kodi.Queries
{
    internal sealed class LibraryReader
    {
        private readonly AmazonDynamoDBClient _db;

        public LibraryReader()
        {
            var config = new AmazonDynamoDBConfig {ServiceURL = "https://dynamodb.eu-west-1.amazonaws.com"};
            _db = new AmazonDynamoDBClient(config);
        }

        public bool TryGetSong(int songId, out Song song)
        {
            try
            {
                var key = new Dictionary<string, AttributeValue> {{"SongId", new AttributeValue{N = songId.ToString()}}};
                var response = _db.GetItemAsync("Songs", key).Result;
                if (response.IsItemSet)
                {
                    var songRef = Guid.Parse(response.Item["SongRef"].S);
                    var artist = response.Item["Artist"].S;
                    var album = response.Item["Album"].S;
                    var title = response.Item["Title"].S;
                    var track = int.Parse(response.Item["Track"].N);
                    var filePath = response.Item["FilePath"].S;
                    song = new Song(songId, songRef, artist, album, title, track, filePath);
                    return true;
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to fetch song (SongId={songId}) from DynamoDb.", e);
            }

            song = null;
            return false;
        }
    }
}