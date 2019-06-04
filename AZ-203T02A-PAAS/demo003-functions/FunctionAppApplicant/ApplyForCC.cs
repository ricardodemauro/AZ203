using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using FunctionAppApplicant.Models;

namespace FunctionAppApplicant
{
    public static class ApplyForCC
    {
        [FunctionName("ApplyForCC")]
        [return: Queue("ccapplication")]
        public static async Task<CCApplication> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string content = await req.ReadAsStringAsync();

            CCApplication ccApplication = JsonConvert.DeserializeObject<CCApplication>(content);

            log.LogInformation($"Received Credit Card Application from : {ccApplication.Name }");

            //await applicationQueue.AddAsync(ccApplication);
            return ccApplication;
        }
    }
}
