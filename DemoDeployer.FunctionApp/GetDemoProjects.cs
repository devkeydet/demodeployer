using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.TeamFoundation.Core.WebApi;

namespace DemoDeployer.FunctionApp
{
    [ExcludeFromCodeCoverage]
    public class DemoProject : TeamProjectReference
    {
        [DataMember(Order = 100)]
        public string MinimumInstanceVersion { get; set; }

        [DataMember(Order = 101)]
        public string OverviewDoc { get; set; }

        [DataMember(Order = 102)]
        public string PrereqsDoc { get; set; }
    }

    internal class DeployableDemo
    {
        public Guid Id { get; set; }
        public string MinimumInstanceVersion { get; set; }
        public string OverviewDoc { get; set; }
        public string PrereqsDoc { get; set; }
    }

    public static class GetDemoProjects
    {

        [ExcludeFromCodeCoverage]
        [FunctionName("GetDemoProjects")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            var projectHttpClient = new ProjectHttpClient(Settings.VstsUri, Settings.VstsCredential);
            return await TestableRun(req, log, projectHttpClient);
        }

        public static async Task<HttpResponseMessage> TestableRun(HttpRequestMessage req, TraceWriter log, ProjectHttpClient projectHttpClient)
        {
            log.Info("C# HTTP trigger function processed a request.");
            List<TeamProjectReference> projects;

            using (projectHttpClient)
            {
                projects = projectHttpClient.GetProjects().Result.ToList();
            }

            // List of VSTS projects that have been identified as deployable demo projects
            // TODO: Build this list in a non hard coded way
            var deployableDemos = new List<DeployableDemo>
            {
                ConfigureDeployableDemo("a177c656-0e57-4173-bfd2-b0c4f9883b21", "8.2", false),  // Treasury Audit Case Management
                ConfigureDeployableDemo("1ba2fb0e-cca7-46c3-b926-7828f224a406", "8.2", false),  // Azure Service Bus and Functions
                ConfigureDeployableDemo("8c561419-0b42-4c8d-83cd-57a1ecf70550", "9.0", true),   // Engagement (ICE))
                ConfigureDeployableDemo("7f588715-6223-4890-9cbc-22bd9f2b6dbd", "9.0", true),   // Correspondence Management
                ConfigureDeployableDemo("a4ce3c38-b6ac-4487-a442-a4cbbd4a5f66", "9.0", false)    // Request Management
            };

            // Only show these for people who are in the "preview" list
            if (AuthorizationHelper.IsUserInPreview(req))
            {
                //deployableDemos.Add(ConfigureDeployableDemo("GUID", "X.X", true/false));    // PROJECTNAME
            }

            // Only return projects who have been "blessed" as a deployable demo
            var demoProjects = from p in projects
                               from dd in deployableDemos
                               where p.Id == dd.Id
                               select new DemoProject
                               {
                                   Id = p.Id,
                                   Name = p.Name,
                                   Description = p.Description,
                                   Url = p.Url,
                                   State = p.State,
                                   Revision = p.Revision,
                                   MinimumInstanceVersion = dd.MinimumInstanceVersion,
                                   OverviewDoc = dd.OverviewDoc,
                                   PrereqsDoc = dd.PrereqsDoc
                               };

            return req.CreateResponse(HttpStatusCode.OK, demoProjects.ToList(), Settings.JsonFormatter);
        }

        private static DeployableDemo ConfigureDeployableDemo(string guidString, string minVersion, bool prereqs)
        {
            const string docsRoot = "https://microsoft.sharepoint.com/teams/USPSFederalDynamics/Shared%20Documents/fedbizappsdemodeployerassets";
            const string openInBrowser = "?Web=1";
            var overviewDoc = $"{docsRoot}/{guidString}/overview.docx{openInBrowser}";
            var prereqsDoc = "";

            if (prereqs)
            {
                prereqsDoc = $"{docsRoot}/{guidString}/prereqs.docx{openInBrowser}";
            }
            return new DeployableDemo { Id = new Guid(guidString), MinimumInstanceVersion = minVersion, OverviewDoc = overviewDoc, PrereqsDoc = prereqsDoc };
        }
    }
}