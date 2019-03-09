using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;

namespace DemoDeployer.FunctionApp.Tests
{
    [TestClass]
    public class GetInstanceVersionTests : AzureFunctionTestBase
    {
        [TestMethod]
        public void TestVersion9Ok()
        {
            var instance = "marcsc-corrmgmt-test";
            var expectedVersion = "9.0";
            TestOk(instance, expectedVersion);
        }

        [TestMethod]
        public void TestVersionZeroOk()
        {
            var instance = "";
            var expectedVersion = "0.0";
            TestOk(instance, expectedVersion);
        }

        private static void TestOk(string instance, string expectedVersion)
        {
            var (fakeRequest, fakeLog) = SetupPost(new
            {
                username = "any@user.onmicrosoft.com",
                password = "",
                instance = instance
            });

            var fakeAdminApiHttpClient = TestHelpers.GetFakeHttpClient("GetInstancesResult.json", HttpStatusCode.OK);
            var fakeAuthHttpClient = TestHelpers.GetFakeHttpClient("GetAccessTokenResult.json", HttpStatusCode.OK);

            var response = GetInstanceVersion.TestableRun(fakeRequest, fakeAuthHttpClient, fakeAdminApiHttpClient).Result;

            Assert.IsInstanceOfType(response, typeof(HttpResponseMessage));
            var jsonString = response.Content.ReadAsStringAsync().Result;
            dynamic r = JsonConvert.DeserializeObject<JObject>(jsonString);

            Assert.AreEqual((string)r.version, expectedVersion);
        }
    }
}