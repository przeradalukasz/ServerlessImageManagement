
using AzureFunctions.Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using RecognitionOrderValidator;
using System.IO;
using System.Threading.Tasks;

namespace ServerlessImageManagement
{
    [DependencyInjectionConfig(typeof(DIConfig))]
    public static class RecognitionStart
    {        
        [FunctionName("RecognitionStart")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequest req, 
            [Queue("recognitionqueue", Connection = "ImageStorageAccount")]IAsyncCollector<RecognitionOrder> queueWithRecOrders, [Inject] IRecOrderValidator validator,
            TraceWriter log)
        {
            string requestBody = new StreamReader(req.Body).ReadToEnd();
            RecognitionOrder recognitionOrder = JsonConvert.DeserializeObject<RecognitionOrder>(requestBody);
            if (!validator.IsValid(recognitionOrder))
            {
                return new BadRequestObjectResult("Provided data is invalid");
            }
            await queueWithRecOrders.AddAsync(recognitionOrder);
            await queueWithRecOrders.FlushAsync();
            return new OkResult();
        }       
    }
}
