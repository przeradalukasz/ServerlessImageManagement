using System.Net;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Net.Http;
using System.Threading.Tasks;

namespace ServerlessImageManagement
{
    public static class UploadImage
    {

        [FunctionName("UploadImage")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "UploadImage/{path}")] HttpRequestMessage req,
            [Queue("thumbnails", Connection = "ImageStorageAccount")] ICollector<string> thumbnailsQueue, string path, TraceWriter log)
        {
            if (req.Content.Headers.ContentLength != 0)
            {
                var container = Utils.BlobClient.GetContainerReference(Utils.WhoAmI());
                var destPath = path.Replace('-', '/');
                CloudBlockBlob newBlob = container.GetBlockBlobReference(destPath);
                if (!await newBlob.ExistsAsync())
                {
                    var imageStream = await req.Content.ReadAsStreamAsync();
                    await newBlob.UploadFromStreamAsync(imageStream);
                    thumbnailsQueue.Add(Utils.GetThumbnailPath(newBlob.Uri.AbsoluteUri));
                    return await Task.FromResult(req.CreateResponse(HttpStatusCode.OK,$"Image {destPath} successfully uploaded"));

                }
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, $"Blob under destination {destPath} already exists");

            }
            return req.CreateErrorResponse(HttpStatusCode.BadRequest, "No image sent");
        }
    }
}
