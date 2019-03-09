using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.VisualStudio.Services.Identity;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Clients;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Contracts;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DemoDeployer.FunctionApp
{
    public static class DeployDemo
    {
        [ExcludeFromCodeCoverage]
        [FunctionName("DeployDemo")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            var releaseHttpClientTestableWrapper = new ReleaseHttpClientTestableWrapper();
            using (var conn = new VssConnection(Settings.VstsUri, Settings.VstsCredential))
            {
                var rmClient = conn.GetClient<ReleaseHttpClient>();
                return await TestableRun(req, log, releaseHttpClientTestableWrapper, rmClient);
            }
        }

        public static async Task<HttpResponseMessage> TestableRun(HttpRequestMessage req, TraceWriter log,
                                                ReleaseHttpClientTestableWrapper releaseHttpClientTestableWrapper,
                                                ReleaseHttpClient rmClient)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // Get request body & variables            
            dynamic content = await req.Content.ReadAsAsync<object>();

            //dynamic content = JsonConvert.DeserializeObject(requestBody);
            string projectId = content.id;
            string dynDomain = content.instance;
            string username = content.username;
            string password = content.password;
            string deployerEmail = content.email;
            bool resetInstance = content.resetInstance;

            var split = username.Split('@');
            var dynUser = split[0];
            var dynTenant = split[1];

            var releaseDefinitions = releaseHttpClientTestableWrapper.GetReleaseDefinitions(projectId, rmClient);

            var releaseDefinition = releaseDefinitions.First();

            var primaryArtifact = releaseDefinition.Artifacts.SingleOrDefault(a => a.IsPrimary);
            var projectName = primaryArtifact.DefinitionReference["project"].Id;
            var buildDefinitionId = Convert.ToInt32(primaryArtifact.DefinitionReference["definition"].Id);

            // Create the draft release, and use the build defined in the current primary artificat
            var metadata = new ReleaseStartMetadata
            {
                DefinitionId = releaseDefinition.Id,
                IsDraft = true,
                Description = $"https://{dynDomain}.crm.dynamics.com ({username}), initiated by {deployerEmail} from Azure Function.",
                Artifacts = new[]
                {
                    new ArtifactMetadata
                    {
                        Alias = primaryArtifact.Alias,
                        InstanceReference = new BuildVersion
                        {
                            Id = primaryArtifact.DefinitionReference["defaultVersionSpecific"].Id,
                            Name = primaryArtifact.DefinitionReference["defaultVersionSpecific"].Name
                        }
                    }
                }
            };
            var release = releaseHttpClientTestableWrapper.CreateRelease(metadata, projectId, rmClient);

            // Update the draft release variable
            var environment = release.Environments[0];
            environment.Variables["dynDomain"].Value = dynDomain;
            environment.Variables["dynUser"].Value = dynUser;
            environment.Variables["dynPassword"].Value = password;
            environment.Variables["dynTenant"].Value = dynTenant;
            environment.Variables["emailNotificationAddress"].Value = deployerEmail;
            environment.Variables["resetInstance"].Value = resetInstance.ToString();

            if (projectId == "1ba2fb0e-cca7-46c3-b926-7828f224a406") // Azure Service Bus and Functions
            {
                var azUniqueName = Regex.Replace(dynDomain, "[^a-zA-Z0-9]", "");
                environment.Variables["azUniqueName"].Value = azUniqueName;
                environment.Variables["azResourceGroup"].Value = $"dd-{azUniqueName}-rg";
            }

            release = releaseHttpClientTestableWrapper.UpdateRelease(release, projectId, release.Id, rmClient);

            // Activate the release
            var updateMetadata = new ReleaseUpdateMetadata { Status = ReleaseStatus.Active, Comment = "Automated by Demo Deployer" };
            release = releaseHttpClientTestableWrapper.UpdateReleaseResource(updateMetadata, projectId, release.Id, rmClient);

            // Issue in PowerApps, which I couldn't get around, is why I'm not sending 200 for success and 400 for errors.
            // https://community.powerapps.com/t5/General-Discussion/Error-handling-with-Custom-API/td-p/7719
            // https://powerusers.microsoft.com/t5/General-Discussion/Extracting-Error-Message-From-PowerApps-Control-and-Store-in/td-p/79494
            dynamic response = new JObject();
            response.status = "success";

            return req.CreateResponse(HttpStatusCode.OK, (JObject)response, Settings.JsonFormatter);
        }
    }

    // Unexplainable issue with faking.  Gave up.  This is a workaround.  
    // TODO: Come back to it
    [ExcludeFromCodeCoverage]
    public class ReleaseHttpClientTestableWrapper
    {
        public virtual List<ReleaseDefinition> GetReleaseDefinitions(string projectId, ReleaseHttpClient rmClient)
        {
            return rmClient.GetReleaseDefinitionsAsync(
                    project: projectId,
                    searchText: "DEMO_DEPLOYER",
                    expand: ReleaseDefinitionExpands.Artifacts
            ).Result;
        }

        public virtual Release CreateRelease(ReleaseStartMetadata metadata, string projectId, ReleaseHttpClient rmClient)
        {
            return rmClient.CreateReleaseAsync(metadata, projectId).Result;
        }

        public virtual Release UpdateRelease(Release release, string projectId, int releaseId, ReleaseHttpClient rmClient)
        {
            return rmClient.UpdateReleaseAsync(release, projectId, releaseId).Result;
        }

        public virtual Release UpdateReleaseResource(ReleaseUpdateMetadata updateMetadata, string projectId, int releaseId, ReleaseHttpClient rmClient)
        {
            return rmClient.UpdateReleaseResourceAsync(updateMetadata, projectId, releaseId).Result;
        }
    }
}