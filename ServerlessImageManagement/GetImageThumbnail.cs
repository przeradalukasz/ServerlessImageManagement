using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ServerlessImageManagement
{
    public static class GetImageThumbnail
    {
        [FunctionName("GetImageThumbnail")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]HttpRequestMessage req,
            [Queue("thumbnails", Connection = "ImageStorageAccount")] ICollector<string> thumbnailsQueue, TraceWriter log)
        {
            Stream photoStream;
            var imagePath = req.GetQueryStrings()["path"];
            string thumbnailPath = Utils.GetThumbnailPath(imagePath);
            try
            {
                var photoBlob = await Utils.BlobClient.GetBlobReferenceFromServerAsync(new Uri(thumbnailPath, UriKind.Absolute));
                photoStream = await photoBlob.OpenReadAsync(AccessCondition.GenerateEmptyCondition(),
                    new BlobRequestOptions(), new OperationContext());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                thumbnailsQueue.Add(thumbnailPath);
                try
                {
                    var photoBlob = await Utils.BlobClient.GetBlobReferenceFromServerAsync(new Uri(imagePath, UriKind.Absolute));
                    photoStream = await photoBlob.OpenReadAsync(AccessCondition.GenerateEmptyCondition(),
                        new BlobRequestOptions(), new OperationContext());
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
                }
            }
            var result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new StreamContent(photoStream);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            return result;
        }

    }
}
