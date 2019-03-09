using FakeItEasy;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace DemoDeployer.FunctionApp.Tests
{
    [TestClass]
    public class GetDemoProjectsTests : AzureFunctionTestBase
    {
        [TestMethod]
        public void TestGetDemoProjectstOk()
        {
            // Arrange
            var (request, fakeLog) = SetupGet();

            var (fakeProjectHttpClient, projects) = SetupVstsApi();

            A.CallTo(() =>
                fakeProjectHttpClient.GetProjects(null, null, null, null, null)).WithAnyArguments()
                .Returns(projects);

            // Act
            var response = GetDemoProjects.TestableRun(request, fakeLog, fakeProjectHttpClient).Result;

            // Assert
            Assert.IsInstanceOfType(response, typeof(HttpResponseMessage));
            var jsonString = response.Content.ReadAsStringAsync().Result;
            var demoProjects = JsonConvert.DeserializeObject<List<DemoProject>>(jsonString);

            Assert.AreEqual(demoProjects.Count, 5);
            Assert.AreEqual(demoProjects[0].OverviewDoc, "https://microsoft.sharepoint.com/teams/USPSFederalDynamics/Shared%20Documents/fedbizappsdemodeployerassets/a177c656-0e57-4173-bfd2-b0c4f9883b21/overview.docx?Web=1");
        }

        [TestMethod]
        public void TesGetDemoProjectstUseInPreviewOk()
        {
            // Arrange
            var (request, fakeLog) = SetupGet();
            var (fakeProjectHttpClient, projects) = SetupVstsApi();

            A.CallTo(() =>
                fakeProjectHttpClient.GetProjects(null, null, null, null, null))
                .Returns(projects);

            // Act
            var response = GetDemoProjects.TestableRun(request, fakeLog, fakeProjectHttpClient).Result;

            // Assert
            Assert.IsInstanceOfType(response, typeof(HttpResponseMessage));
            var jsonString = response.Content.ReadAsStringAsync().Result;
            var demoProjects = JsonConvert.DeserializeObject<List<DemoProject>>(jsonString);

            Assert.AreEqual(demoProjects.Count, 5);
            Assert.AreEqual(demoProjects[0].OverviewDoc, "https://microsoft.sharepoint.com/teams/USPSFederalDynamics/Shared%20Documents/fedbizappsdemodeployerassets/a177c656-0e57-4173-bfd2-b0c4f9883b21/overview.docx?Web=1");
        }

        private static (ProjectHttpClient, PagedList<TeamProjectReference>) SetupVstsApi()
        {
            Environment.SetEnvironmentVariable("VstsUrl", "https://someaccount.visualstudio.com", EnvironmentVariableTarget.Process);

            var fakeProjectHttpClient = A.Fake<ProjectHttpClient>(p => p.WithArgumentsForConstructor(() =>
                  new ProjectHttpClient(Settings.VstsUri, Settings.VstsCredential)));
            var projects = new PagedList<TeamProjectReference>
            {
                new TeamProjectReference
                {
                    Id = new Guid("a177c656-0e57-4173-bfd2-b0c4f9883b21"),
                    Name = "Treasury Audit Case Management"
                },
                new TeamProjectReference
                {
                    Id = new Guid("1ba2fb0e-cca7-46c3-b926-7828f224a406"),
                    Name = "Azure Service Bus and Functions"
                },
                new TeamProjectReference
                {
                    Id = new Guid("8c561419-0b42-4c8d-83cd-57a1ecf70550"),
                    Name = "Engagement (ICE)"
                },
                new TeamProjectReference
                {
                    Id = new Guid("7f588715-6223-4890-9cbc-22bd9f2b6dbd"),
                    Name = "Correspondence Management"
                },
                new TeamProjectReference
                {
                    Id = new Guid("a4ce3c38-b6ac-4487-a442-a4cbbd4a5f66"),
                    Name = "Request Management"
                }
            };

            return (fakeProjectHttpClient, projects);
        }
    }
}