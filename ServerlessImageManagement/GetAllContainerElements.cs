
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using ServerlessImageManagement.DTO;

namespace ServerlessImageManagement
{
    public static class GetAllContainerElements
    {
        [FunctionName("GetAllContainerElements")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetAllContainerElements/{userId}")]HttpRequest req,
            [Blob("{userId}", FileAccess.Read, Connection = "ImageStorageAccount")] CloudBlobContainer userStorage, string userId, TraceWriter log)
        {
            await userStorage.CreateIfNotExistsAsync();
            var blobs = await userStorage.ListBlobsSegmentedAsync(String.Empty, true, BlobListingDetails.All, Int32.MaxValue, null, new BlobRequestOptions(), new OperationContext());
            var paths = blobs.Results.Where(x => !x.Uri.AbsoluteUri.Contains("_thumbnail."))
                .Select(e => e.Uri.AbsoluteUri);
            

            var root = new TreeElement() { Name = userId, Children = new List<TreeElement>() };

            foreach (var path in paths)
            {
                var splitBetweenBaseAndDirectory = path.IndexOf(userId, StringComparison.Ordinal) + userId.Length;
                var basePath = path.Substring(0, splitBetweenBaseAndDirectory);
                var parts = path.Remove(0, splitBetweenBaseAndDirectory + 1).Split('/');
             
                BuildTree(root, parts, basePath);
            }
            return new OkObjectResult(JsonConvert.SerializeObject(root));
        }

        public static void BuildTree(TreeElement result, IEnumerable<string> remainingElements, string basePath)
        {
            var elementsList = remainingElements as IList<string> ?? remainingElements.ToList();
            if (elementsList.Any())
            {
                var name = elementsList.First();
                var child = result.Children.SingleOrDefault(x => x.Name == name);
                if (child == null)
                {
                    child = new TreeElement()
                    {
                        Name = name,
                        Children = new List<TreeElement>(),
                        AbsolutePath = basePath + '/' + name
                    };
                    result.Children.Add(child);
                }
                BuildTree(child, elementsList.Skip(1), child.AbsolutePath);
            }
        }
    }
}
