using System.IO;
using System.Net;
using Amazon.S3;
using HoneyBear.Alexa.Kodi.Models;

namespace HoneyBear.Alexa.Kodi.Queries
{
    internal sealed class CacheReader
    {
        private readonly AmazonS3Client _s3;

        public CacheReader()
        {
            _s3 = new AmazonS3Client();
        }

        public bool TryGet(string key, out string json)
        {
            try
            {
                var response = _s3.GetObjectAsync(Song.S3Bucket, key).Result;
                if (response.HttpStatusCode == HttpStatusCode.OK)
                {
                    using (var reader = new StreamReader(response.ResponseStream))
                        json = reader.ReadToEnd();
                    return true;
                }
            }
            catch
            {
                // ignored
            }

            json = null;
            return false;
        }
    }
}