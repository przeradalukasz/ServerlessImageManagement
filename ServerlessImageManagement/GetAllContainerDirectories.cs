
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace ServerlessImageManagement
{
    public static class GetAllContainerDirectories
    {
        [FunctionName("GetAllContainerDirectories")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetAllContainerDirectories/{userId}")]HttpRequest req,
            [Blob("{userId}", FileAccess.Read, Connection = "ImageStorageAccount")] CloudBlobContainer userStorage, string userId, TraceWriter log)
        {
            await userStorage.CreateIfNotExistsAsync();
            var blobs = await userStorage.ListBlobsSegmentedAsync(String.Empty, true, BlobListingDetails.All, Int32.MaxValue, null, new BlobRequestOptions(), new OperationContext());
            var paths = blobs.Results.Select(e => e.Parent)
                .Select(e => e.Uri.AbsolutePath.TrimEnd('/')).Distinct();

            var root = paths.GetHierarchy(userId);
            return new OkObjectResult(JsonConvert.SerializeObject(root));
        }
    }
}
