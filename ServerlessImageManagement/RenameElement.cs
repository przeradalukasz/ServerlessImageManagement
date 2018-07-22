using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ServerlessImageManagement
{
    public static class RenameElement
    {
        [FunctionName("RenameElement")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            string requestBody = await req.Content.ReadAsStringAsync();
            var fullPath = Utils.GetParamValueFromHttpBody(requestBody, "path");

            var containerName = Utils.GetContainerNameFromFullPath(Utils.BlobClient.BaseUri, fullPath);
            var container = Utils.BlobClient.GetContainerReference(containerName);

            var oldFileName = Utils.GetDirectoryFromFullPath(fullPath, containerName) + Path.GetFileName(fullPath);
            var newFileName = Utils.GetDirectoryFromFullPath(fullPath, containerName) + Utils.GetParamValueFromHttpBody(requestBody, "newFileName");

            var oldFileThumbnailName = Utils.GetThumbnailPath(oldFileName);
            var newFileThumbnailName = Utils.GetThumbnailPath(newFileName);

            if (await container.ExistsAsync())
            {
                CloudBlockBlob newBlob = container.GetBlockBlobReference(newFileName);
                CloudBlockBlob newBlobThumbnail = container.GetBlockBlobReference(newFileThumbnailName);

                if (!await newBlob.ExistsAsync())
                {
                    CloudBlockBlob oldBlob = container.GetBlockBlobReference(oldFileName);
                    CloudBlockBlob oldBlobThumbnail = container.GetBlockBlobReference(oldFileThumbnailName);
                    
                    if (await oldBlob.ExistsAsync())
                    {
                        await newBlob.StartCopyAsync(oldBlob);
                        await newBlobThumbnail.StartCopyAsync(oldBlobThumbnail);
                        await oldBlob.DeleteIfExistsAsync();
                        await oldBlobThumbnail.DeleteIfExistsAsync();
                        return req.CreateResponse(HttpStatusCode.OK, "Name successfully changed");

                    }
                    return req.CreateErrorResponse(HttpStatusCode.BadRequest, $"Could not find blob with name: {Path.GetFileName(fullPath)}");
                }
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, $"Blob with the name {newFileName} already exists");
            }
            return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not find the container for this user");
        }
    }
}
