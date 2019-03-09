using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Net;

namespace DemoDeployer.FunctionApp
{
    public static class TestDynDomain
    {
        [ExcludeFromCodeCoverage]
        [FunctionName("TestDynDomain")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            var client = new HttpClient();
            return await TestableRun(req, log, client);
        }

        public static async Task<HttpResponseMessage> TestableRun(HttpRequestMessage req, TraceWriter log, HttpClient client)
        {
            log.Info("C# HTTP trigger function processed a request.");
            dynamic body = await req.Content.ReadAsAsync<object>();
            var instance = (string)body.instance;
            var dynUrl = $"https://{instance}.crm.dynamics.com";
            bool success = true;

            using (client)
            {
                try
                {
                    var result = client.GetAsync(dynUrl).Result;
                }
                catch (System.Exception)
                {
                    success = false;
                }
            }

            // Issue in PowerApps, which I couldn't get around, is why I'm not sending 200 for success and 400 for errors.
            // https://community.powerapps.com/t5/General-Discussion/Error-handling-with-Custom-API/td-p/7719
            // https://powerusers.microsoft.com/t5/General-Discussion/Extracting-Error-Message-From-PowerApps-Control-and-Store-in/td-p/79494
            dynamic response = new JObject();
            if (success)
            {
                response.status = "success";
            }
            else
            {
                response.status = "Invalid Dynamics 365 Instance";
            }

            return req.CreateResponse(HttpStatusCode.OK, (JObject)response, Settings.JsonFormatter);
        }
    }
}