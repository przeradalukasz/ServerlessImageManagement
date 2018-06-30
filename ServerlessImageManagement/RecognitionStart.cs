
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using ServerlessImageManagement.DTO;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ServerlessImageManagement
{
    public static class RecognitionStart
    {
        private static readonly Regex Regex = new Regex(@"^(\+[0-9]{9})$", RegexOptions.Compiled);
        private static readonly EmailAddressAttribute EmailAddressAttribute = new EmailAddressAttribute();

        [FunctionName("RecognitionStart")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequest req, 
            [Queue("recognitionqueue", Connection = "ImageStorageAccount")]IAsyncCollector<RecognitionOrder> queueWithRecOrders, 
            TraceWriter log)
        {
            string requestBody = new StreamReader(req.Body).ReadToEnd();
            RecognitionOrder recognitionOrder = JsonConvert.DeserializeObject<RecognitionOrder>(requestBody);
            if (!IsValid(recognitionOrder))
            {
                return new BadRequestObjectResult("Provided data is invalid");
            }
            await queueWithRecOrders.AddAsync(recognitionOrder);
            await queueWithRecOrders.FlushAsync();
            return new OkResult();
        }

        private static bool IsValid(RecognitionOrder recognitionOrder)
        {
            if (string.IsNullOrWhiteSpace(recognitionOrder.DestinationFolder))
                return false;
            if (string.IsNullOrWhiteSpace(recognitionOrder.PhotosSource))
                return false;
            if (!recognitionOrder.PatternFaces.Any())
                return false;
            if (string.IsNullOrWhiteSpace(recognitionOrder.EmailAddress) || !EmailAddressAttribute.IsValid(recognitionOrder.EmailAddress))
                return false;
            if (!Regex.Match(recognitionOrder.PhoneNumber).Success)
                return false;
            return true;
        }
    }
}
