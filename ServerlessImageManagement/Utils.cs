using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace ServerlessImageManagement
{
    public static class Utils
    {
        public static readonly CloudBlobClient BlobClient;

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
            var newFileName = Path.GetFileNameWithoutExtension(imagePath) + "_thumbnail.jpg";
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
            var relativePath = fullPath.Substring(blobClientBaseUri.AbsoluteUri.Length + 1);
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
    }
}
