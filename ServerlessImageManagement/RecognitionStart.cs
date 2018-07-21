using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using ServerlessImageManagement.DTO;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ServerlessImageManagement
{
    public static class RecognitionStart
    {
        private static readonly Regex PhoneRegex = new Regex(@"^((00[0-9]{2})|(\+[0-9]{2}))?([0-9]{9})$", RegexOptions.Compiled);
        private static readonly EmailAddressAttribute EmailAddressAttribute = new EmailAddressAttribute();

        [FunctionName("RecognitionStart")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequestMessage req, 
            [Queue("recognitionqueue", Connection = "ImageStorageAccount")]IAsyncCollector<RecognitionOrder> queueWithRecOrders, 
            TraceWriter log)
        {
            string requestBody = await req.Content.ReadAsStringAsync();
            RecognitionOrder recognitionOrder = JsonConvert.DeserializeObject<RecognitionOrder>(requestBody);
            if (!IsValid(recognitionOrder))
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Provided data is invalid");
            }
            await queueWithRecOrders.AddAsync(recognitionOrder);
            await queueWithRecOrders.FlushAsync();
            return req.CreateResponse(HttpStatusCode.OK);
        }

        private static bool IsValid(RecognitionOrder recognitionOrder)
        {
            if (string.IsNullOrWhiteSpace(recognitionOrder.DestinationFolder))
                return false;
            if (string.IsNullOrWhiteSpace(recognitionOrder.SourcePath))
                return false;
            if (!recognitionOrder.PatternFaces.Any())
                return false;
            if (string.IsNullOrWhiteSpace(recognitionOrder.EmailAddress) || !EmailAddressAttribute.IsValid(recognitionOrder.EmailAddress))
                return false;
            if (!PhoneRegex.Match(recognitionOrder.PhoneNumber).Success)
                return false;
            return true;
        }
    }
}
