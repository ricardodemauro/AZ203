using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using FaceDetectionV3.Infrastructure;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace FaceDetectionV3
{
    public class FaceDetectionFunction
    {
        private readonly FaceApp _faceApp;

        private readonly StorageOptions _storageOptions;

        private readonly ILogger<FunctionDetect> _logger;

        public FaceDetectionFunction(FaceApp faceApp, IOptions<StorageOptions> storageOptions, ILogger<FunctionDetect> logger)
        {
            _faceApp = faceApp ?? throw new ArgumentNullException(nameof(faceApp));
            _storageOptions = storageOptions?.Value ?? throw new ArgumentNullException(nameof(storageOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [FunctionName("FaceDetection")]
        public async Task Run(
            [BlobTrigger("photos/{name}", Connection = "face_blob")]Stream inputBlob,
            string name,
            ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {inputBlob.Length} Bytes");

            log.LogInformation("Calling face detection api");

            using (MemoryStream memStream = new MemoryStream())
            {
                await inputBlob.CopyToAsync(memStream, 1024);

                inputBlob.Seek(0, SeekOrigin.Begin);
                memStream.Seek(0, SeekOrigin.Begin);

                var result = await _faceApp.DetectFaceExtract(memStream);

                using (var image = Image.FromStream(inputBlob))
                {
                    Graphics graph = Graphics.FromImage(image);

                    graph.Clear(Color.Azure);

                    Pen pen = new Pen(Brushes.Black);

                    graph.DrawLines(pen, new Point[] { new Point(result.Left, result.Top), new Point(result.Left + result.Width, result.Top + result.Height) });

                    Rectangle rect = new Rectangle(100, 100, 300, 300);
                    graph.DrawRectangle(pen, rect);


                    using (MemoryStream memStreamThumb = new MemoryStream())
                    {
                        image.Save(memStreamThumb, System.Drawing.Imaging.ImageFormat.Png);
                        memStreamThumb.Seek(0, SeekOrigin.Begin);

                        await CreateBlockBlobAsync(name, memStreamThumb);
                    }
                }
            }
        }


        async Task CreateBlockBlobAsync(string blobName, MemoryStream stream)
        {
            // Construct the blob container endpoint from the arguments.
            string containerEndpoint = string.Format("https://{0}.blob.core.windows.net/{1}",
                                                        _storageOptions.StorageAccountName,
                                                        _storageOptions.ContainerName);

            // Get a credential and create a client object for the blob container.
            BlobContainerClient containerClient = new BlobContainerClient(
                new Uri(containerEndpoint),
                new DefaultAzureCredential());

            try
            {
                await containerClient.CreateIfNotExistsAsync();

                await containerClient.UploadBlobAsync(blobName, stream);
            }
            catch (RequestFailedException e)
            {
                _logger.LogError(e, "Error when trying to write blob");
                throw;
            }
        }
    }
}
