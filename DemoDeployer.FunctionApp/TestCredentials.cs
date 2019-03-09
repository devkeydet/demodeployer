using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Net;

namespace DemoDeployer.FunctionApp
{
    public static class TestCredentials
    {
        [ExcludeFromCodeCoverage]
        [FunctionName("TestCredentials")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            var whoAmIHttpClient = new HttpClient();
            var authHttpClient = new HttpClient();
            return await TestableRun(req, log, whoAmIHttpClient, authHttpClient);
        }

        public static async Task<HttpResponseMessage> TestableRun(HttpRequestMessage req, TraceWriter log, HttpClient whoAmIHttpClient, HttpClient authHttpClient)
        {
            log.Info("C# HTTP trigger function processed a request.");

            //string requestBody = await req.Content.ReadAsAsync<string>();
            //dynamic body = JsonConvert.DeserializeObject(requestBody);
            dynamic body = await req.Content.ReadAsAsync<object>();
            // Set name to query string or body data
            var username = (string)body.username;
            var password = (string)body.password;
            var instance = (string)body.instance;
            var dynUrl = $"https://{instance}.crm.dynamics.com";
            bool success;

            try
            {
                string accessToken;
                accessToken = AuthenticationHelper.GetAccessToken(username, password, dynUrl, authHttpClient);

                using (whoAmIHttpClient)
                {
                    whoAmIHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    var whoAmIResult = whoAmIHttpClient.GetAsync($"{dynUrl}/api/data/v8.1/WhoAmI").Result;
                    success = whoAmIResult.IsSuccessStatusCode;
                }
            }
            catch (Exception e)
            {
                success = false;
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
                response.status = $"Authentication failed for user {username} against the following instance: {dynUrl}.  Please make sure username and password are correct and the user is authorized to access the instance.";
            }

            return req.CreateResponse(HttpStatusCode.OK, (JObject)response, Settings.JsonFormatter);
        }
    }
}