using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;


namespace ServerlessImageManagement
{
    public static class CreateThumbnail
    {
        [FunctionName("CreateThumbnail")]
        public static async Task Run([QueueTrigger("thumbnails", Connection = "ImageStorageAccount")] string thumbnailPath,
            [Blob("{queueTrigger}", Connection = "ImageStorageAccount")] ICloudBlob resizedPhotoCloudBlob, TraceWriter log)
        {
            try
            {
                var imagePath = Utils.GetImagePathFromThumbnail(thumbnailPath);
                var photoBlob = await Utils.BlobClient.GetBlobReferenceFromServerAsync(new Uri(imagePath, UriKind.Absolute));
                var photoStream = await photoBlob.OpenReadAsync(AccessCondition.GenerateEmptyCondition(),
                    new BlobRequestOptions(), new OperationContext());

                //resize logic
                var resizedPhotoStream = photoStream;

                //-----

                await resizedPhotoCloudBlob.UploadFromStreamAsync(resizedPhotoStream);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

    }
}
