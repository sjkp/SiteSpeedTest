using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace SJKP.SiteSpeedTest.Models
{
    public class SpeedTestJob
    {
        public string Id { get; set; }
        public Region Region { get; set; }

        public string SubscriptionId { get; set; }

        public Uri Uri { get; set; }

        public string ResourceGroupName { get; set; }
    }
}
