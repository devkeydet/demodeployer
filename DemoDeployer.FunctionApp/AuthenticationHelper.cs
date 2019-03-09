using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;

namespace DemoDeployer.FunctionApp
{
    public static class AuthenticationHelper
    {
        public static string GetAccessToken(string username, string password, string resource, HttpClient authClient)
        {
            using (authClient)
            {
                var authContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("resource", resource),
                    new KeyValuePair<string, string>("client_id", Settings.AuthTesterClientId),
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("username", username),
                    new KeyValuePair<string, string>("password", password),
                    new KeyValuePair<string, string>("scope", "openid"),
                    new KeyValuePair<string, string>("client_secret", Settings.AuthTesterClientSecret),
                });
                var authResult = authClient.PostAsync("https://login.microsoftonline.com/common/oauth2/token", authContent).Result;
               
                if (authResult.IsSuccessStatusCode)
                {
                    var responseContent = authResult.Content.ReadAsStringAsync().Result;
                    dynamic responseObject = JsonConvert.DeserializeObject(responseContent);
                    return (string)responseObject.access_token;
                }
                else
                {
                    return "";
                }
            }
        }
    }
}