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
    public class FaceDetectionFunctionV2
    {
        private readonly FaceApp _faceApp;

        private readonly StorageOptions _storageOptions;

        private readonly ILogger<FunctionDetect> _logger;

        public FaceDetectionFunctionV2(FaceApp faceApp, IOptions<StorageOptions> storageOptions, ILogger<FunctionDetect> logger)
        {
            _faceApp = faceApp ?? throw new ArgumentNullException(nameof(faceApp));
            _storageOptions = storageOptions?.Value ?? throw new ArgumentNullException(nameof(storageOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [FunctionName("FaceDetectionV2")]
        public void Run(
            [BlobTrigger("photos/{name}", Connection = "face_blob")]Stream inputBlob,
            string name,
            [Blob("thumb/{name}", FileAccess.Write)] Stream outBlob,
            ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {inputBlob.Length} Bytes");

            log.LogInformation("Calling face detection api");

            using (MemoryStream memStream = new MemoryStream())
            {
                inputBlob.CopyTo(memStream, 1024);

                inputBlob.Seek(0, SeekOrigin.Begin);
                memStream.Seek(0, SeekOrigin.Begin);

                var result = _faceApp.DetectFaceExtract(memStream)
                    .GetAwaiter()
                    .GetResult();

                using (var image = Image.FromStream(inputBlob))
                {
                    Graphics graph = Graphics.FromImage(image);

                    Pen pen = new Pen(Brushes.Black);

                    graph.DrawLines(pen, new Point[] { new Point(result.Left, result.Top), new Point(result.Left + result.Width, result.Top + result.Height) });

                    Rectangle rect = new Rectangle(100, 100, 300, 300);
                    graph.DrawRectangle(pen, rect);

                    using (MemoryStream stream = new MemoryStream())
                    {
                        image.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);

                        stream.Seek(0, SeekOrigin.Begin);

                        stream.CopyTo(outBlob);
                    }

                }
            }
        }
    }
}
