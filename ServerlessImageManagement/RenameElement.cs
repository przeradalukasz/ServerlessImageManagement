
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
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
    public static class RenameElement
    {
        [FunctionName("RenameElement")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = null)]HttpRequest req, TraceWriter log)
        {
            string requestBody = new StreamReader(req.Body).ReadToEnd();
            var fullPath = Utils.GetParamValueFromHttpBody(requestBody, "path");

            var containerName = Utils.GetContainerNameFromFullPath(Utils.BlobClient.BaseUri, fullPath);
            var container = Utils.BlobClient.GetContainerReference(containerName);

            var oldFileName = Utils.GetDirectoryFromFullPath(fullPath, containerName) + '/' + Path.GetFileName(fullPath);
            var newFileName = Utils.GetDirectoryFromFullPath(fullPath, containerName) + '/' + Utils.GetParamValueFromHttpBody(requestBody, "newFileName");

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
                        return new OkObjectResult("Name successfully changed");

                    }
                    return new BadRequestErrorMessageResult($"Could not find blob with name: {Path.GetFileName(fullPath)}");
                }
                return new BadRequestErrorMessageResult($"Blob with the name {newFileName} already exists");
            }
            return new BadRequestErrorMessageResult("Could not find the container for this user");
        }
    }
}
