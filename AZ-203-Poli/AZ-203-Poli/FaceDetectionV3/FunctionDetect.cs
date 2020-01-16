using FaceDetectionV3.Infrastructure;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FaceDetectionV3
{
    public class FunctionDetect
    {
        private readonly FaceApp _faceApp;

        private readonly ILogger<FunctionDetect> _logger;

        public FunctionDetect(FaceApp faceApp, ILogger<FunctionDetect> logger)
        {
            _faceApp = faceApp ?? throw new ArgumentNullException(nameof(faceApp));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [FunctionName("FunctionDetect")]
        public async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await req.Content.ReadAsStringAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string baseUri = data?.baseUri;

            var result = await _faceApp.DetectFaceExtract(baseUri);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(result), Encoding.UTF8, "application/json")
            };

        }


    }
}
