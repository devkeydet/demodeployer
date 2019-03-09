using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.VisualStudio.Services.Identity;

namespace DemoDeployer.FunctionApp
{
    public static class KeepFunctionAppRunning
    {
        [ExcludeFromCodeCoverage]
        static KeepFunctionAppRunning()
        {
            // Workaround to address issue described here:
            // https://developercommunity.visualstudio.com/content/problem/117356/problem-using-vsts-c-client-library-vssconnection.html
            //
            // This issue manifests itself in the DeployDemo function when calling conn.GetClient<ReleaseHttpClient>();
            //
            // GitHub issue filed:
            // https://github.com/Azure/Azure-Functions/issues/817

            ResolveEventHandler handler = null;

            handler = (sender, args) =>
            {
                AppDomain.CurrentDomain.AssemblyResolve -= handler;

                if (args.Name.StartsWith("Microsoft.VisualStudio.Services.WebApi"))
                {
                    return typeof(IdentityDescriptor).Assembly;
                }
                return null;
            };

            AppDomain.CurrentDomain.AssemblyResolve += handler;
        }

        [ExcludeFromCodeCoverage]
        [FunctionName("KeepFunctionAppRunning")]
        public static void Run([TimerTrigger("0 */5 * * * *", RunOnStartup = true)]TimerInfo myTimer, TraceWriter log)
        {
            // used to keep the consumption based azure funtion loaded for perf
            log.Info($"Keep alive timer trigger function executed at: {DateTime.Now}");
        }
    }
}