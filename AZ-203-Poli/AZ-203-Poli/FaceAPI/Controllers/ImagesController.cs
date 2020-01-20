using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using FaceAPI.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        private async ValueTask<BlobContainerClient> GetCloudBlobContainer(string containerName)
        {
            _logger.LogInformation("Getting Container Reference for {container}", containerName);

            BlobServiceClient blobServiceClient = new BlobServiceClient(_options.StorageConnectionString);

            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

            return containerClient;
        }

        private async Task<IEnumerable<string>> GetFilesFromContainer(string containerName)
        {
            var container = await GetCloudBlobContainer(containerName);

            List<BlobItem> results = new List<BlobItem>();

            await foreach (BlobItem item in container.GetBlobsAsync())
            {
                results.Add(item);
            }

            _logger.LogInformation("Got Images from {container}", containerName);

            return results.Select(blob => $"{_options.BaseUrl}/{containerName}/{blob.Name}");
        }

        //generate SAS Keys
        private string UrlWithSAS(string filename, string containerName)
        {
            var builder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                StartsOn = DateTime.UtcNow,
                ExpiresOn = DateTime.UtcNow.AddMinutes(4),
                BlobName = filename
            };

            builder.SetPermissions(BlobAccountSasPermissions.Read);

            //  Builds an instance of StorageSharedKeyCredential      
            var storageSharedKeyCredential = new StorageSharedKeyCredential(_options.AccountName, _options.AccountKey);

            var sasQueryParameters = builder.ToSasQueryParameters(storageSharedKeyCredential);

            //  Builds the URI to the blob storage.
            UriBuilder fullUri = new UriBuilder()
            {
                Scheme = "https",
                Host = string.Format("{0}.blob.core.windows.net", _options.AccountName),
                Path = string.Format("{0}/{1}", containerName, filename),
                Query = sasQueryParameters.ToString()
            };

            _logger.LogInformation("Uri with SAS Token generated for {filename} and {container}", filename, containerName);

            return fullUri.ToString();
        }

        [HttpGet()]
        public async Task<ActionResult<IEnumerable<string>>> Index()
        {
            return Ok(await GetFilesFromContainer(_options.FullImageContainerName));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> Thumbs()
        {
            return Ok(await GetFilesFromContainer(_options.ThumbnailImageContainerName));
        }

        [HttpPost()]
        public async Task<ActionResult> Create(string filename = null)
        {
            Stream image = Request.Body;

            string blobName = filename ?? $"{Guid.NewGuid().ToString().ToLower().Replace("-", string.Empty)}.png";

            var containerClient = await GetCloudBlobContainer(_options.FullImageContainerName);

            // Get a reference to a blob
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            _logger.LogInformation("Uploading to Blob storage as blob: {uri}", blobClient.Uri);

            var pvd = new FileExtensionContentTypeProvider();
            _ = pvd.TryGetContentType(blobName, out string mimeType);

            // Open the file and upload its data
            await blobClient.UploadAsync(image, new BlobHttpHeaders()
            {
                ContentType = mimeType
            });

            return Created(blobClient.Name, null);
        }
    }
}
