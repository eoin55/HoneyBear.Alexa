using System;
using Amazon.S3;
using Amazon.S3.Model;
using HoneyBear.Alexa.Kodi.Models;

namespace HoneyBear.Alexa.Kodi.Commands
{
    internal sealed class CacheWriter
    {
        private readonly AmazonS3Client _s3;

        public CacheWriter()
        {
            _s3 = new AmazonS3Client();
        }

        public void Save(string json, string key)
        {
            try
            {
                var request = new PutObjectRequest
                {
                    BucketName = Song.S3Bucket,
                    Key = key,
                    ContentBody = json,
                    ContentType = "application/json"
                };
                var response = _s3.PutObjectAsync(request).Result;
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to upload {key} to S3 cache.", e);
            }
        }
    }
}