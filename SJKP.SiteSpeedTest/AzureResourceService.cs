using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SJKP.SiteSpeedTest
{
    public class AzureResourceService : ServiceBase
    {
        public async Task<IList<string>> GetLocations(string subscriptionid)
        {
            var auth = await Authenticate();

            var containerInstances = ResourceManager.Authenticate(auth).WithSubscription(subscriptionid).Providers.GetByName("Microsoft.ContainerInstance");
                        
            return containerInstances.ResourceTypes.Where(s => s.ResourceType == "containerGroups").SelectMany(s => s.Locations).ToList();
        }

        public async Task<IPagedCollection<ISubscription>> GetSubscriptions()
        {
            var auth = await GetRestClient();

            var subscriptions = await ResourceManager.Authenticate(auth).Subscriptions.ListAsync();

            return subscriptions;
        }

        public async Task EnsureResourceGroup(string subscriptionid, string name, Region region)
        {
            var auth = await Authenticate();
            var mgr = ResourceManager.Authenticate(auth).WithSubscription(subscriptionid);
            var rg = await mgr.ResourceGroups.ContainAsync(name);

            if (!rg)
            {
                await mgr.ResourceGroups.Define(name).WithRegion(region).CreateAsync();
            }
        }
    }
}
