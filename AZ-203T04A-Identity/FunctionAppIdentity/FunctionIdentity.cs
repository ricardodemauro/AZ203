using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace FunctionAppIdentity
{
    public static class FunctionIdentity
    {
        public static async Task<HttpResponseMessage> GetToken(string resource, ILogger log)
        {
            string endpoint = Environment.GetEnvironmentVariable("MSI_ENDPOINT");
            string secret = Environment.GetEnvironmentVariable("MSI_SECRET");

            log.LogInformation($"endpoing {endpoint}");
            log.LogInformation($"secret {secret}");

            using (HttpClient _client = new HttpClient())
            {
                log.LogInformation($"Uri {endpoint}?resource={resource}&api-version=2017-09-01");
                var request = new HttpRequestMessage(HttpMethod.Get, $"{endpoint}?resource={resource}&api-version=2017-09-01");
                request.Headers.Add("Secret", secret);

                return await _client.SendAsync(request);
            }
        }

        [FunctionName("FuncIdentity")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogInformation($"Body --> {requestBody}");

            SecretRequest secretRequest = JsonConvert.DeserializeObject<SecretRequest>(requestBody);

            if (string.IsNullOrEmpty(secretRequest.Secret))
                return new BadRequestObjectResult("Request does not contain a valid Secret.");

            log.LogInformation($"GetKeyVaultSecret request received for secret { secretRequest.Secret}");

            var serviceTokenProvider = new AzureServiceTokenProvider();

            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(serviceTokenProvider.KeyVaultTokenCallback));

            string secretUri = SecretUri((string)secretRequest.Secret);
            log.LogInformation($"Key Vault URI {secretUri} generated");
            SecretBundle secretValue;
            try
            {
                secretValue = await keyVaultClient.GetSecretAsync(secretUri);
            }
            catch (KeyVaultErrorException kex)
            {
                return new NotFoundObjectResult(kex.Message);
            }
            log.LogInformation("Secret Value retrieved from KeyVault.");

            var secretResponse = new SecretResponse { Secret = secretRequest.Secret, Value = secretValue.Value };

            return new OkObjectResult(JsonConvert.SerializeObject(secretResponse));
        }

        public class SecretRequest
        {
            [JsonProperty("secret")]
            public string Secret { get; set; }
        }

        public class SecretResponse
        {
            public string Secret { get; set; }

            public string Value { get; set; }
        }

        public static string SecretUri(string secret)
        {
            var uri = Environment.GetEnvironmentVariable("KeyVaultUri");
            return $"{uri}secrets/{secret}";
        }
    }
}
