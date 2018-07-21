using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;

namespace ServerlessImageManagement
{
    public static class Utils
    {
        public static readonly CloudBlobClient BlobClient;
        public static readonly string userName = "dummyuser";

        static Utils()
        {
            BlobClient = CloudStorageAccount.Parse(Utils.GetEnvironmentVariable("ImageStorageAccount"))
                .CreateCloudBlobClient();
        }

        public static string GetEnvironmentVariable(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }

        public static string GetThumbnailPath(string imagePath)
        {
            var extenstion = Path.GetExtension(imagePath);
            var newFileName = Path.GetFileNameWithoutExtension(imagePath) + "_thumbnail" + extenstion;
            return imagePath.Replace(Path.GetFileName(imagePath), newFileName);
        }

        public static string GetImagePathFromThumbnail(string thumbnailPath)
        {
            return thumbnailPath.ReplaceLastOccurrence("_thumbnail", String.Empty);
        }

        public static string GetParamValueFromHttpBody(string requestBody, string paramKey)
        {
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            return data[paramKey];
        }

        public static string GetContainerNameFromFullPath(Uri blobClientBaseUri, string fullPath)
        {
            var offset = 1;
            if(blobClientBaseUri.AbsoluteUri[blobClientBaseUri.AbsoluteUri.Length-1] == '/')
            {
                offset = 0;
            }
            var relativePath = fullPath.Substring(blobClientBaseUri.AbsoluteUri.Length + offset);
            return relativePath.Substring(0, relativePath.IndexOf('/'));
        }

        public static string GetDirectoryFromFullPath(string fullPath, string containerName)
        {
            var directoryFilePath = fullPath.Substring(fullPath.IndexOf(containerName, StringComparison.Ordinal) + containerName.Length + 1);
            return directoryFilePath.Substring(0, directoryFilePath.LastIndexOf('/'));
        }

        private static string ReplaceLastOccurrence(this string source, string find, string replace)
        {
            int place = source.LastIndexOf(find, StringComparison.Ordinal);

            if (place == -1)
                return source;
            string result = source.Remove(place, find.Length).Insert(place, replace);
            return result;
        }

        public static Dictionary<string, string> GetQueryStrings(this HttpRequestMessage request)
        {
            return request.GetQueryNameValuePairs()
                .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
        }

        public static string WhoAmI()
        {
            return "dummyuser4";
        }
    }
}
