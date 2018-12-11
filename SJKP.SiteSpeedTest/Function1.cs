using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using System.Net.Http;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Text.RegularExpressions;

namespace SJKP.SiteSpeedTest
{
    public static class Function1
    {
        const string subscriptionId = "3f09c367-93e0-4b61-bbe5-dcb5c686bf8a";

        [FunctionName("start")]
        public static async Task<IActionResult> StartTest(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var body = await req.Content.ReadAsAsync<StartSpeedTest>();

            var region = Region.Create(body.Region);
            if (!Region.Values.Contains(region))
            {
                return new BadRequestObjectResult($"Invalid region, valid values are {string.Join(",", Region.Values)}");
            }
            
            if (!Uri.TryCreate(body.WebSite, UriKind.Absolute, out var url))
            {
                return new BadRequestObjectResult("Url invalid");
            }

            var name = $"{Regex.Replace(url.IdnHost, "[^\\w]", "-")}-{region.Name}-speedtest-{Guid.NewGuid()}";

            starter.StartNewAsync()
            var id = await new AciService().StartNewSpeedTest(subscriptionId, region, url, "speedtest");

            return new OkObjectResult(id);
        }

        [FunctionName("locations")]
        public static async Task<IActionResult> GetLocations(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "subscriptions/{id}/containerGroups/locations")] HttpRequestMessage req, string id,
           ILogger log)
        {
            return new OkObjectResult(await new AzureResourceService().GetLocations(id));
        }

        [FunctionName("subscriptions")]
        public static async Task<IActionResult> GetSubscriptions(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "subscriptions")] HttpRequestMessage req, 
           ILogger log)
        {
            return new OkObjectResult(await new AzureResourceService().GetSubscriptions());
        }


        [FunctionName("status")]
        public static async Task<IActionResult> GetStatus(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "sitestatus/{id}/region/{region}/status")] HttpRequestMessage req, string region, string id,
           ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var body = await req.Content.ReadAsAsync<StartSpeedTest>();

            var r = Region.Create(region);
            if (!Region.Values.Contains(r))
            {
                return new BadRequestObjectResult($"Invalid region, valid values are {string.Join(",", Region.Values)}");
            }

            
            var status = await new AciService().Status(subscriptionId, r, id);

            return new OkObjectResult(status);
        }

        [FunctionName("browse")]
        public static async Task<HttpResponseMessage> Browse(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "browse/{*path}")] HttpRequestMessage req, string path, [Blob("test/{path}", Connection = "AzureWebJobsStorage")] CloudBlockBlob file,
           ILogger log)
        {

            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
          
            
            response.Content = new StreamContent(await file.OpenReadAsync());
            response.Content.Headers.ContentLength = file.Properties.Length;
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.Properties.ContentType);
            //response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
            //{
            //    FileName = "dataFile.zip",
            //    Size = file.Length
            //};


            return response;
        }


        [FunctionName("folderlist")]
        public static async Task<ActionResult> FolderList(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "folder/{*path}")] HttpRequestMessage req, string path, [Blob("test", FileAccess.Read, Connection = "AzureWebJobsStorage")] CloudBlobContainer container,
           ILogger log)
        {

                var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            BlobResultSegment files = null;
            if (path == null)
            {
                files = await container.ListBlobsSegmentedAsync(null, false, BlobListingDetails.None, 500, new BlobContinuationToken()
                {

                }, new BlobRequestOptions(), new Microsoft.WindowsAzure.Storage.OperationContext());
            }
            else
            {

                files = await container.GetDirectoryReference(path).ListBlobsSegmentedAsync(false, BlobListingDetails.None, 500, new BlobContinuationToken()
                {

                }, new BlobRequestOptions(), new Microsoft.WindowsAzure.Storage.OperationContext());

            }
            return new OkObjectResult(files.Results.Where(s => s is CloudBlobDirectory).Select( s=> (s as CloudBlockBlob)?.Name ?? (s as CloudBlobDirectory)?.Prefix));
        }
    }
}
