using Autofac;
using AzureFunctions.Autofac.Configuration;
using RecognitionOrderValidator;

namespace ServerlessImageManagement
{
    public class DIConfig
    {
        public DIConfig(string functionName)
        {
            DependencyInjection.Initialize(builder =>
            {
                builder.RegisterType<RecOrderValidator>().As<IRecOrderValidator>();
            }, functionName);
        }
    }
}