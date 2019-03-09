using FakeItEasy;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Clients;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;

namespace DemoDeployer.FunctionApp.Tests
{
    public class PostBody
    {
        public string id;
        public string instance;
        public string username;
        public string password;
        public string email;
        public bool resetInstance;
    }

    [TestClass]
    public class DemployDemoTests : AzureFunctionTestBase
    {
        [TestMethod]
        public void TestDeployDemoNoAzureOk()
        {
            // Arrange
            var postBody = new PostBody
            {
                id = "someguid",
                instance = "some-dyn-instance",
                username = "some@user.com",
                password = "anything",
                email = "some@email.com",
                resetInstance = false,
            };

            var environment = new ReleaseEnvironment();

            TestDeployDemo(postBody, environment);
        }

        [TestMethod]
        public void TestDeployAzureWithAzureOk()
        {
            // Arrange
            var postBody = new PostBody
            {
                id = "1ba2fb0e-cca7-46c3-b926-7828f224a406",
                instance = "some-dyn-instance",
                username = "some@user.com",
                password = "anything",
                email = "some@email.com",
                resetInstance = false,
            };

            var environment = new ReleaseEnvironment();
            environment.Variables.Add("azUniqueName", new ConfigurationVariableValue());
            environment.Variables.Add("azResourceGroup", new ConfigurationVariableValue());

            TestDeployDemo(postBody, environment);

            Assert.AreEqual(environment.Variables["azUniqueName"].Value, "somedyninstance");
            Assert.AreEqual(environment.Variables["azResourceGroup"].Value, "dd-somedyninstance-rg");
        }

        public void TestDeployDemo(PostBody postBody, ReleaseEnvironment environment)
        {
            var (fakeRequest, fakeLog) = SetupPost(postBody);

            Environment.SetEnvironmentVariable("VstsUrl", "https://someaccount.visualstudio.com", EnvironmentVariableTarget.Process);

            var releaseDefinitions = JsonConvert.DeserializeObject<List<ReleaseDefinition>>(
                File.ReadAllText("GetReleaseDefinitionsResult.json")
            );

            var fakeReleaseHttpClientTestableWrapper = A.Fake<ReleaseHttpClientTestableWrapper>();
            A.CallTo(() =>
                fakeReleaseHttpClientTestableWrapper.GetReleaseDefinitions(null, null)).WithAnyArguments()
                .Returns(releaseDefinitions);

            var release = new Release();
            release.Environments.Add(environment);

            environment.Variables.Add("dynDomain", new ConfigurationVariableValue());
            environment.Variables.Add("dynUser", new ConfigurationVariableValue());
            environment.Variables.Add("dynPassword", new ConfigurationVariableValue());
            environment.Variables.Add("dynTenant", new ConfigurationVariableValue());
            environment.Variables.Add("emailNotificationAddress", new ConfigurationVariableValue());
            environment.Variables.Add("resetInstance", new ConfigurationVariableValue());

            A.CallTo(() =>
                fakeReleaseHttpClientTestableWrapper.CreateRelease(null, null, null)).WithAnyArguments()
                .Returns(release);

            A.CallTo(() =>
                fakeReleaseHttpClientTestableWrapper.UpdateRelease(null, null, 0, null)).WithAnyArguments()
                .Returns(release);

            A.CallTo(() =>
                fakeReleaseHttpClientTestableWrapper.UpdateReleaseResource(null, null, 0, null)).WithAnyArguments()
                .Returns(release);

            var fakeReleaseHttpClient = A.Fake<ReleaseHttpClient>(x => x.WithArgumentsForConstructor(() =>
                  new ReleaseHttpClient(Settings.VstsUri, Settings.VstsCredential)));

            // Act
            var response = DeployDemo.TestableRun(fakeRequest, fakeLog,
                                                  fakeReleaseHttpClientTestableWrapper,
                                                  fakeReleaseHttpClient).Result;

            // Assert
            Assert.IsInstanceOfType(response, typeof(HttpResponseMessage));
            //var result = (OkObjectResult)response;
            var jsonString = response.Content.ReadAsStringAsync().Result;
            dynamic r = JsonConvert.DeserializeObject<JObject>(jsonString);
            //dynamic r = (JObject)result.Value;

            Assert.AreEqual((string)r.status, "success");
            Assert.AreEqual(environment.Variables["dynDomain"].Value, postBody.instance);
            Assert.AreEqual(environment.Variables["dynUser"].Value, "some");
            Assert.AreEqual(environment.Variables["dynPassword"].Value, postBody.password);
            Assert.AreEqual(environment.Variables["dynTenant"].Value, "user.com");
            Assert.AreEqual(environment.Variables["emailNotificationAddress"].Value, postBody.email);
            Assert.AreEqual(environment.Variables["resetInstance"].Value, postBody.resetInstance.ToString());
        }
    }
}