using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;

namespace DemoDeployer.FunctionApp
{
    internal static class AuthorizationHelper
    {
        [ExcludeFromCodeCoverage]
        internal static bool IsUserInPreview(HttpRequestMessage req)
        {
            try
            {
                var user = req.Headers.GetValues("X-MS-CLIENT-PRINCIPAL-NAME").FirstOrDefault();
                // TODO: Build this list in a non hard coded way
                var previewUsers = new List<string>
                {
                    "someone@yourtenant.onmicrosoft.com"
                };

                if (previewUsers.Contains(user))
                {
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                // Exception means we are running in local runtime and this value isn't present.  The value is always present when using Azure App Service Auth
                // https://docs.microsoft.com/en-us/azure/app-service/app-service-authentication-overview
                // Assume, when running local, we see preview features.
                return true;
            }
        }
    }
}