using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FaceAPI.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FaceAPI.Controllers
{
    public class ImagesController : Controller
    {
        private readonly StorageOptions _options;
        private readonly ILogger<ImagesController> _logger;

        public ImagesController(IOptions<StorageOptions> options, ILogger<ImagesController> logger)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async ValueTask<CloudBlobContainer> GetCloudBlobContainer(string containerName)
        {
            CloudStorageAccount account = CloudStorageAccount.Parse(_options.StorageConnectionString);
            CloudBlobClient blobClient = account.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            await container.CreateIfNotExistsAsync();
            return container;
        }

        [HttpGet()]
        public async Task<ActionResult<IEnumerable<string>>> Index()
        {
            CloudBlobContainer container = await GetCloudBlobContainer(_options.FullImageContainerName);
            BlobContinuationToken continuationToken = null;

            List<IListBlobItem> results = new List<IListBlobItem>();
            do
            {
                var response = await container.ListBlobsSegmentedAsync(continuationToken);
                continuationToken = response.ContinuationToken;
                results.AddRange(response.Results);
            }
            while (continuationToken != null);

            _logger.LogInformation("Got Images");

            return Ok(results.Select(blob => blob.Uri.AbsoluteUri));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> Thumbs()
        {
            CloudBlobContainer container = await GetCloudBlobContainer(_options.ThumbnailImageContainerName);
            BlobContinuationToken continuationToken = null;

            List<IListBlobItem> results = new List<IListBlobItem>();
            do
            {
                var response = await container.ListBlobsSegmentedAsync(continuationToken);
                continuationToken = response.ContinuationToken;
                results.AddRange(response.Results);
            }
            while (continuationToken != null);

            _logger.LogInformation("Got Thumbs");

            return Ok(results.Select(blob => blob.Uri.AbsoluteUri));
        }

        [Route("")]
        [HttpPost]
        public async Task<ActionResult> Create()
        {
            Stream image = Request.Body;

            CloudBlobContainer container = await GetCloudBlobContainer(_options.FullImageContainerName);
            string blobName = Guid.NewGuid().ToString().ToLower().Replace("-", String.Empty);

            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);
            await blockBlob.UploadFromStreamAsync(image);

            return Created(blockBlob.Uri, null);
        }
    }
}
