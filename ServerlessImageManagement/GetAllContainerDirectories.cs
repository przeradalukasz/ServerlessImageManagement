using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ServerlessImageManagement
{
    public static class GetAllContainerDirectories
    {
        [FunctionName("GetAllContainerDirectories")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetAllContainerDirectories/{userId}")]HttpRequestMessage req,
            [Blob("{userId}", FileAccess.Read, Connection = "ImageStorageAccount")] CloudBlobContainer userStorage, string userId, TraceWriter log)
        {
            await userStorage.CreateIfNotExistsAsync();
            var blobs = await userStorage.ListBlobsSegmentedAsync(String.Empty, true, BlobListingDetails.All, Int32.MaxValue, null, new BlobRequestOptions(), new OperationContext());
            var paths = blobs.Results.Select(e => e.Parent)
                .Select(e => e.Uri.AbsoluteUri.TrimEnd('/')).Distinct();

            var root = paths.GetHierarchy(userId);
            return req.CreateResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(root));
        }
    }
}
