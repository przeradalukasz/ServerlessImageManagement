
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Web.Http;

namespace ServerlessImageManagement
{
    public static class DeleteElement
    {

        [FunctionName("DeleteElement")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = null)]HttpRequest req, TraceWriter log)
        {
            var fullPath = req.Query["path"];
            try
            {
                var blobToDelete = await Utils.BlobClient.GetBlobReferenceFromServerAsync(new Uri(fullPath, UriKind.Absolute));
                var blobThumbnailToDelete = await Utils.BlobClient.GetBlobReferenceFromServerAsync(new Uri(Utils.GetThumbnailPath(fullPath), UriKind.Absolute));
                await blobToDelete.DeleteIfExistsAsync();
                await blobThumbnailToDelete.DeleteIfExistsAsync();
                return new OkObjectResult($"Element {Path.GetFileName(fullPath)} successfully deleted");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new BadRequestErrorMessageResult($"Could not find the blob - {Path.GetFileName(fullPath)}");
            }
        }
    }
}
