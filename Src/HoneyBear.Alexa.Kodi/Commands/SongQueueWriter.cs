using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using HoneyBear.Alexa.Kodi.Models;
using HoneyBear.Alexa.Kodi.Queries;

namespace HoneyBear.Alexa.Kodi.Commands
{
    internal sealed class SongQueueWriter
    {
        private readonly AmazonDynamoDBClient _db;
        private readonly SongQueueReader _queue;

        public SongQueueWriter()
        {
            var config = new AmazonDynamoDBConfig {ServiceURL = "https://dynamodb.eu-west-1.amazonaws.com"};
            _db = new AmazonDynamoDBClient(config);
            _queue = new SongQueueReader();
        }

        public void Clear()
        {
            foreach (var song in _queue.QueuedSongs())
                Delete(song);
        }

        public void Save(ICollection<Song> songs)
        {
            Clear();
            foreach (var song in songs)
                Insert(song);
        }
        
        private void Insert(Song song)
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
                var response = _db.PutItemAsync(new PutItemRequest("SongQueue", items)).Result;
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to save song ({song}) to queue to DynamoDb.", e);
            }
        }

        private void Delete(Song song)
        {
            try
            {
                var items = new Dictionary<string, AttributeValue>
                {
                    {"SongId", new AttributeValue {N = song.SongId.ToString()}}
                };
                var response = _db.DeleteItemAsync("SongQueue", items).Result;
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to delete song ({song}) from queue to DynamoDb.", e);
            }
        }
    }
}