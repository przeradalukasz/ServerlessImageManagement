using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ServerlessImageManagement
{
    public static class DeleteElement
    {

        [FunctionName("DeleteElement")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            var fullPath = req.GetQueryStrings()["path"];
            try
            {
                var blobToDelete = await Utils.BlobClient.GetBlobReferenceFromServerAsync(new Uri(fullPath, UriKind.Absolute));
                var blobThumbnailToDelete = await Utils.BlobClient.GetBlobReferenceFromServerAsync(new Uri(Utils.GetThumbnailPath(fullPath), UriKind.Absolute));
                await blobToDelete.DeleteIfExistsAsync();
                await blobThumbnailToDelete.DeleteIfExistsAsync();
                return req.CreateResponse(HttpStatusCode.OK, $"Element {Path.GetFileName(fullPath)} successfully deleted");
            }
            catch (Exception e)
            {
                log.Error("Delete element failed", e);
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, $"Could not find the blob - {Path.GetFileName(fullPath)}");
            }
        }
    }
}
