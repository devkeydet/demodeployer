using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DemoDeployer.FunctionApp.Tests
{
    [TestClass]
    public class AuthenticationHelperTests
    {
        [TestMethod]
        public void TestNoToken()
        {
            // Other tests cover the other paths
            var fakeAuthHttpClient = TestHelpers.GetFakeHttpClientNoResponseContent(System.Net.HttpStatusCode.NotFound);
            var token = AuthenticationHelper.GetAccessToken("", "", "", fakeAuthHttpClient);
            Assert.AreEqual(token,"");
        }
    }
}
