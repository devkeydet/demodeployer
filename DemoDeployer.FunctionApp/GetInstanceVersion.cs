using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Net;

namespace DemoDeployer.FunctionApp
{
    public static class GetInstanceVersion
    {
        [FunctionName("GetInstanceVersion")]
        [ExcludeFromCodeCoverage]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            var authHttpClient = new HttpClient();
            var adminApiHttpClient = new HttpClient();
            return await TestableRun(req, authHttpClient, adminApiHttpClient);
        }

        public static async Task<HttpResponseMessage> TestableRun(HttpRequestMessage req, HttpClient authHttpClient, HttpClient adminApiHttpClient)
        {
            dynamic body = await req.Content.ReadAsAsync<object>();
            
            // Set name to query string or body data
            var username = (string)body.username;
            var password = (string)body.password;
            var instance = (string)body.instance;

            const string adminApiUrl = "https://admin.services.crm.dynamics.com/";
            const string adminApiVersion = "api/v1";
            var version = "0.0";


            try
            {
                string accessToken;
                accessToken = AuthenticationHelper.GetAccessToken(username, password, adminApiUrl, authHttpClient);

                if (accessToken != "")
                {
                    using (adminApiHttpClient)
                    {
                        adminApiHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                        var json = adminApiHttpClient.GetStringAsync($"{adminApiUrl}/{adminApiVersion}/Instances").Result;
                        var instances = JArray.Parse(json);
                        version = (from i in instances
                                    where i["DomainName"].ToString().ToLower() == instance.ToLower()
                                    select i["Version"]).First().Value<string>();

                        var versionArray = version.Split('.');
                        version = $"{versionArray[0]}.{versionArray[1]}";
                    }
                }
            }
            catch (Exception e)
            {
                // Eat the exception.  Function will just return 0.0
            }

            dynamic response = new JObject();
            response.version = version;

            return req.CreateResponse(HttpStatusCode.OK, (JObject)response, Settings.JsonFormatter);
        }
    }
}