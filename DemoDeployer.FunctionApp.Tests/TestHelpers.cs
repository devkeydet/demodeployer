using System.IO;
using System.Net;
using System.Net.Http;
using WorldDomination.Net.Http;

namespace DemoDeployer.FunctionApp.Tests
{
    class TestHelpers
    {
        public static HttpClient GetFakeHttpClient(string fileName, HttpStatusCode httpStatus)
        {
            var json = File.ReadAllText(fileName);
            var responseData = FakeHttpMessageHandler.GetStringHttpResponseMessage(json);
            responseData.StatusCode = httpStatus;
            var options = new HttpMessageOptions { HttpResponseMessage = responseData, };
            var fakeMessageHandler = new FakeHttpMessageHandler(options);

            return new HttpClient(fakeMessageHandler);
        }
        public static HttpClient GetFakeHttpClientNoResponseContent(HttpStatusCode httpStatus)
        {
            var responseData = FakeHttpMessageHandler.GetStringHttpResponseMessage("");
            responseData.StatusCode = httpStatus;
            var options = new HttpMessageOptions { HttpResponseMessage = responseData, };
            var fakeMessageHandler = new FakeHttpMessageHandler(options);

            return new HttpClient(fakeMessageHandler);
        }
    }
}
