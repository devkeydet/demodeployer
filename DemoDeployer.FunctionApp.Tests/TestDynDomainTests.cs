using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;

namespace DemoDeployer.FunctionApp.Tests
{
    [TestClass]
    public class TestDynDomainTests : AzureFunctionTestBase
    {
        [TestMethod]
        public void TestValidDomainOk()
        {
            var instance = "marcsc-corrmgmt-test";
            var expectedResult = "success";
            TestOk(instance, expectedResult);
        }

        [TestMethod]
        public void TestInValidDomainOk()
        {
            var instance = "";
            var expectedResult = "Invalid Dynamics 365 Instance";
            TestOk(instance, expectedResult);
        }

        private static void TestOk(string instance, string expectedResult)
        {
            var (fakeRequest, fakeLog) = SetupPost(new
            {
                instance = instance
            });

            var fakeHttpClient = TestHelpers.GetFakeHttpClientNoResponseContent(HttpStatusCode.OK);

            var response = TestDynDomain.TestableRun(fakeRequest, fakeLog, fakeHttpClient).Result;

            Assert.IsInstanceOfType(response, typeof(HttpResponseMessage));
            var jsonString = response.Content.ReadAsStringAsync().Result;
            dynamic r = JsonConvert.DeserializeObject<JObject>(jsonString);

            Assert.AreEqual((string)r.status, expectedResult);
        }
    }
}