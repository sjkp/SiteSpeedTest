using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.ContainerInstance.Fluent.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Rest;
using Microsoft.WindowsAzure.Storage;
using SJKP.SiteSpeedTest.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SJKP.SiteSpeedTest
{
    public class AciService : ServiceBase
    {        
        public async Task StartNewSpeedTest(SpeedTestJob speedTest)
        {
            var mgr = await Authenticate(speedTest.SubscriptionId);
            var account = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("AzureWebJobsStorage")).Credentials;
            var accountName = account.AccountName;
            var pass = account.ExportBase64EncodedKey();            

            await new AzureResourceService().EnsureResourceGroup(speedTest.SubscriptionId, speedTest.ResourceGroupName, speedTest.Region);

            var containerGroup = await mgr.ContainerGroups.Define(speedTest.Id).WithRegion(speedTest.Region).WithExistingResourceGroup(speedTest.ResourceGroupName)
                .WithLinux().WithPublicImageRegistryOnly()
                //.WithoutVolume()
                .WithEmptyDirectoryVolume("azurefiles")
                .DefineContainerInstance("minio").WithImage("minio/minio")
                .WithInternalTcpPort(9000).WithStartingCommandLine("minio", "gateway", "azure")
                .WithEnvironmentVariables(new Dictionary<string, string>
                {
                    {"MINIO_ACCESS_KEY",accountName},
                    {"MINIO_SECRET_KEY", pass }
                })
                .WithCpuCoreCount(0.5)
                .WithMemorySizeInGB(0.5)
                .Attach()                
                .DefineContainerInstance("speedtest").WithImage("sjkp/sitespeedtest")              
                .WithoutPorts()
                .WithVolumeMountSetting("azurefiles", "/sitespeed.io")
                .WithEnvironmentVariables(new Dictionary<string, string>()
                {
                    { "SITESPEEDIO_OUTPUT", "test" },
                    { "SITESPEEDIO_MINIO_HOST", "http://127.0.0.1:9000" },
                    { "SITESPEEDIO_MINIO_LOGIN", accountName },
                    { "SITESPEEDIO_MINIO_PASS", pass}
                })
                .WithStartingCommandLine("/start.sh", speedTest.Uri.ToString()).Attach()                
                .WithRestartPolicy(ContainerGroupRestartPolicy.Never).CreateAsync();           
        }

        public async Task<ContainerStatus> Status(string subscriptionId, Region region, string id)
        {
            var mgr = await Authenticate(subscriptionId);
            var containerGroup = await mgr.ContainerGroups.GetByResourceGroupAsync(id, "speedtest");
            var logContent = containerGroup.GetLogContent("speedtest");
            return new ContainerStatus
            {
                ProvisioningState = containerGroup.ProvisioningState,
                LogContent = logContent
            };
        }

        protected async Task<IContainerInstanceManager> Authenticate(string subscriptionId)
        {
            var creds = await base.Authenticate();
            return ContainerInstanceManager.Authenticate(creds, subscriptionId);
        }
    }
}
