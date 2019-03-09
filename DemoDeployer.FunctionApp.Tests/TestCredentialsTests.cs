using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;

namespace DemoDeployer.FunctionApp.Tests
{
    [TestClass]
    public class TestCredentialsTests : AzureFunctionTestBase
    {
        const string USERNAME = "some@user.com";
        const string INSTANCE = "any-instance-name";
        readonly string INVALID_EXPECTED_RESULTS = $"Authentication failed for user {USERNAME} against the following instance: https://{INSTANCE}.crm.dynamics.com.  Please make sure username and password are correct and the user is authorized to access the instance.";

        [TestMethod]
        public void TestValidCredentialsOk()
        {
            var expectedResult = "success";
            TestOk(expectedResult, HttpStatusCode.OK, false);
        }

        [TestMethod]
        public void TestInValidCredentialsOk()
        {
            var expectedResult = INVALID_EXPECTED_RESULTS;
            TestOk(expectedResult, HttpStatusCode.Unauthorized, false);
        }

        [TestMethod]
        public void TestnValidCredentialsCatchesExceptionOk()
        {
            var expectedResult = INVALID_EXPECTED_RESULTS;
            TestOk(expectedResult, HttpStatusCode.Unauthorized, true);
        }

        private static void TestOk(string expectedResult, HttpStatusCode statusCode, bool causeException)
        {
            var (fakeRequest, fakeLog) = SetupPost(new
            {
                instance = INSTANCE,
                username = USERNAME,
                password = "somepassword"
            });

            var fakeWhoAmIHttpClient = TestHelpers.GetFakeHttpClientNoResponseContent(statusCode);
            var fakeAuthHttpClient = TestHelpers.GetFakeHttpClient("GetAccessTokenResult.json", HttpStatusCode.OK);

            if (causeException)
            {
                fakeAuthHttpClient = null;
                fakeWhoAmIHttpClient = null;
            }

            var response = TestCredentials.TestableRun(fakeRequest, fakeLog, fakeWhoAmIHttpClient, fakeAuthHttpClient).Result;

            Assert.IsInstanceOfType(response, typeof(HttpResponseMessage));
            var jsonString = response.Content.ReadAsStringAsync().Result;
            dynamic r = JsonConvert.DeserializeObject<JObject>(jsonString);

            Assert.AreEqual((string)r.status, expectedResult);
        }
    }
}