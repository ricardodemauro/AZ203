using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;

namespace FunctionAppIdentity
{
    public class FunctionIdentityToken
    {
        public static async Task<HttpResponseMessage> GetToken(string resource)
        {
            string endpoint = Environment.GetEnvironmentVariable("MSI_ENDPOINT");
            string secret = Environment.GetEnvironmentVariable("MSI_SECRET");

            var request = new HttpRequestMessage(HttpMethod.Get, $"{endpoint}/?resource={resource}&api-version=2017-09-01");
            request.Headers.Add("Secret", secret);

            using (var client = new HttpClient())
            {
                return await client.SendAsync(request);
            }
        }

        [FunctionName("FunctionIdentityToken")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string resource = Environment.GetEnvironmentVariable("TargetResourceUri");
            var response = await GetToken(resource);

            return new OkObjectResult($"Hello, {await response.Content.ReadAsStringAsync()}");
        }
    }
}
