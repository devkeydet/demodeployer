using FakeItEasy;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace DemoDeployer.FunctionApp.Tests
{
    public abstract class AzureFunctionTestBase

    {
        private static(HttpRequestMessage, TraceWriter) Setup()
        {
            var request = new HttpRequestMessage();
            request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            var log = A.Fake<TraceWriter>();
            return (request, log);
        }

        public static (HttpRequestMessage, TraceWriter) SetupPost(object content)

        {
            var (request, log) = Setup();
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri("http://tempuri.org");
            request.Content = new StringContent(
                JsonConvert.SerializeObject(content),
                Encoding.UTF8, "application/json");
            //A.CallTo(() => fakeHttpRequestMessage.Content).Returns(req.Content.ReadAsStreamAsync().Result);

            //A.CallTo(() => request.Content.ReadAsAsync<object>()).Returns(request.Content.ReadAsStreamAsync().Result);

            return (request, log);
        }

        public static (HttpRequestMessage, TraceWriter) SetupGet()

        {
            var (fakeHttpRequestMessage, log) = Setup();
            return (fakeHttpRequestMessage, log);
        }

        //public static (HttpRequestMessage, TraceWriter) SetupGet(IEnumerable<KeyValuePair<string, string>> queryStringCollection)

        //{
        //    var (request, log) = Setup();
        //    var iQueryCollection = ToIQueryCollection(queryStringCollection);
        //    A.CallTo(() => request.Query.Query).Returns(iQueryCollection);

        //    return (request, log);
        //}

        //static IQueryCollection ToIQueryCollection(IEnumerable<KeyValuePair<string, string>> iekvpss)
        //{
        //    var queryCollection = new Dictionary<string, StringValues>();
        //    foreach (var item in iekvpss)
        //    {
        //        queryCollection.Add(item.Key, new StringValues(item.Value));
        //    }

        //    return new QueryCollection(queryCollection);
        //}
    }
}
