using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Rest;
using System.Threading.Tasks;

namespace SJKP.SiteSpeedTest
{
    public class ServiceBase
    {
        private static readonly AzureServiceTokenProvider _tokenProvider = new AzureServiceTokenProvider();
        protected async Task<AzureCredentials> Authenticate()
        {
            var env = AzureEnvironment.AzureGlobalCloud;
            string accessToken = await _tokenProvider.GetAccessTokenAsync(env.ResourceManagerEndpoint).ConfigureAwait(false);
            var creds = new AzureCredentials(new TokenCredentials(accessToken), new TokenCredentials(accessToken), _tokenProvider.PrincipalUsed.TenantId, env);
            return creds;
        }

        protected async Task<RestClient> GetRestClient()
        {

            var auth = await Authenticate();
            var client = RestClient.Configure().WithEnvironment(AzureEnvironment.AzureGlobalCloud).WithCredentials(auth).Build();
            return client;
        }
    }
}
