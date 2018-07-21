using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
namespace ServerlessImageManagement
{
    public static class GetAllContainerElements
    {
        [FunctionName("GetAllContainerElements")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]HttpRequestMessage req,
             IBinder binder, TraceWriter log)
        {
            var userId = Utils.WhoAmI();
            var attribute = new BlobAttribute($"{userId}", FileAccess.Read);
            attribute.Connection = "ImageStorageAccount";

            var userStorage = await binder.BindAsync<CloudBlobContainer>(attribute);
            await userStorage.CreateIfNotExistsAsync();
            var blobs = await userStorage.ListBlobsSegmentedAsync(String.Empty, true, BlobListingDetails.All, Int32.MaxValue, null, new BlobRequestOptions(), new OperationContext());
            var paths = blobs.Results.Where(x => !x.Uri.AbsoluteUri.Contains("_thumbnail."))
                .Select(e => e.Uri.AbsoluteUri);
            var root = paths.GetHierarchy(userId);
            return req.CreateResponse(HttpStatusCode.OK, root);
        }
    }
}
