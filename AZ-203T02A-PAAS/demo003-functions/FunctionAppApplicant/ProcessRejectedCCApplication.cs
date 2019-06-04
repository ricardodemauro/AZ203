using System.IO;
using FunctionAppApplicant.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FunctionAppApplicant
{
    public static class ProcessRejectedCCApplication
    {
        [FunctionName("ProcessRejectedCCApplication")]
        public static void Run([BlobTrigger("rejected-application/{name}", Connection = "")]string ccApplicationJson, string name, ILogger log)
        {
            CCApplication ccApplication = JsonConvert.DeserializeObject<CCApplication>(ccApplicationJson);
            log.LogInformation($"ProcessRejectedCCApplication Blob Trigger for \n Name:{ccApplication.Name} with file name {name}");
        }
    }
}
