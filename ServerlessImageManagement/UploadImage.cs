using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace ServerlessImageManagement
{
    public static class UploadImage
    {

        [FunctionName("UploadImage")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "UploadImage/{userId}/{path}")] HttpRequestMessage req,
            [Queue("thumbnails", Connection = "ImageStorageAccount")] ICollector<string> thumbnailsQueue, string userId, string path, TraceWriter log)
        {
            if (req.Content.Headers.ContentLength != 0)
            {
                var container = Utils.BlobClient.GetContainerReference(userId);
                var destPath = path.Replace('-', '/');
                CloudBlockBlob newBlob = container.GetBlockBlobReference(destPath);
                if (!await newBlob.ExistsAsync())
                {
                    var imageStream = await req.Content.ReadAsStreamAsync();
                    await newBlob.UploadFromStreamAsync(imageStream);
                    thumbnailsQueue.Add(Utils.GetThumbnailPath(newBlob.Uri.AbsoluteUri));
                    return await Task.FromResult(new OkObjectResult($"Image {destPath} successfully uploaded"));

                }
                return new BadRequestErrorMessageResult($"Blob under destination {destPath} already exists");

            }
            return new BadRequestObjectResult("No image sent");
        }
    }
}
