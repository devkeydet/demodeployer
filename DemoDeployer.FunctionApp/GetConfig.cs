using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;

namespace DemoDeployer.FunctionApp
{
    public class Config
    {
        public string AuthTesterUrl { get; set; }
        public string HeaderFillColor { get; set; }
    }

    public static class GetConfig
    {
        [FunctionName("GetConfig")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");
            var config = new Config
            {
                AuthTesterUrl = Settings.AuthTesterUrl,
                HeaderFillColor = Settings.HeaderFillColor
            };

            return req.CreateResponse(HttpStatusCode.OK, config, Settings.JsonFormatter);
        }
    }
}