using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace DemoDeployer.FunctionApp.Tests
{
    [TestClass]
    public class GetConfigTests : AzureFunctionTestBase
    {
        [TestMethod]
        public void TestOkAsync()
        {
            var (req, log) = SetupGet();

            var response = GetConfig.Run(req, log).Result;

            Assert.IsInstanceOfType(response, typeof(HttpResponseMessage));
            var jsonString = response.Content.ReadAsStringAsync().Result;
            var config = JsonConvert.DeserializeObject<Config>(jsonString);

            Assert.IsNull(config.AuthTesterUrl);
            Assert.IsNull(config.HeaderFillColor);
        }

        //[TestMethod]
        //public void TestFakeGetWithQueryStringOk()
        //{
        //    var queryStringParameters = new List<KeyValuePair<string, string>>
        //    {
        //        new KeyValuePair<string, string>("foo","800")
        //    };

        //    var (req, log) = SetupGet(queryStringParameters);

        //    var response = GetConfig.Run(req, log);

        //    Assert.IsInstanceOfType(response, typeof(OkObjectResult));
        //    var result = (OkObjectResult)response;
        //    var config = (Config)result.Value;

        //    Assert.IsNull(config.AuthTesterUrl);
        //    Assert.IsNull(config.HeaderFillColor);
        //}
    }
}
