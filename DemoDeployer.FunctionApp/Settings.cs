using Microsoft.VisualStudio.Services.Common;
using Newtonsoft.Json.Serialization;
using System;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace DemoDeployer.FunctionApp
{
    public static class Settings
    {
        internal static JsonMediaTypeFormatter JsonFormatter => GetJsonMediaTypeFormatter();
        static string VstsUrl => Get("VstsUrl");
        static string PersonalAccessToken => Get("PersonalAccessToken");
        public static string AuthTesterClientId => Get("AuthTesterClientId");
        public static string AuthTesterClientSecret => Get("AuthTesterClientSecret");
        public static string AuthTesterUrl => Get("AuthTesterUrl");
        public static string HeaderFillColor => Get("HeaderFillColor");
        public static Uri VstsUri => new Uri(VstsUrl);
        public static VssBasicCredential VstsCredential => GetVstsCredential();

        private static string Get(string name)
        {
            return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }

        private static VssBasicCredential GetVstsCredential()
        {
            var credentials = new VssBasicCredential(string.Empty, PersonalAccessToken);
            return credentials;
        }

        private static JsonMediaTypeFormatter GetJsonMediaTypeFormatter()
        {
            var formatter = new JsonMediaTypeFormatter();
            var settings = formatter.SerializerSettings;
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            return formatter;
        }
    }
}