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

        private readonly ILogger<FunctionDetect> _logger;

        public FaceDetectionFunctionV2(FaceApp faceApp, ILogger<FunctionDetect> logger)
        {
            _faceApp = faceApp ?? throw new ArgumentNullException(nameof(faceApp));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [FunctionName("FaceDetectionV2")]
        public async Task Run(
            [BlobTrigger("photos/{name}", Connection = "face_blob")]Stream inputBlob,
            string name,
            [Blob("thumb/{name}", FileAccess.Write)] Stream outBlob,
            ILogger log)
        {
            log.LogInformation("C# Blob trigger function Processed blob Name {name} and Size of {size} Bytes", name, inputBlob.Length);

            using (MemoryStream memStream = new MemoryStream())
            {
                await inputBlob.CopyToAsync(memStream, 1024);

                inputBlob.Seek(0, SeekOrigin.Begin);
                memStream.Seek(0, SeekOrigin.Begin);

                log.LogInformation("Calling face detection api");
                var result = await _faceApp.DetectFaceExtract(memStream);

                using (var image = Image.FromStream(inputBlob))
                {
                    Graphics graph = Graphics.FromImage(image);

                    Pen pen = new Pen(Brushes.Red, 1.7f);

                    graph.DrawRectangle(pen, new Rectangle(result.Left, result.Top, result.Width, result.Height));
                    
                    _logger.LogInformation("found face like left {left} top {top} width {width} and height {height}", 
                        result.Left, 
                        result.Top, 
                        result.Width, 
                        result.Height);

                    using (MemoryStream stream = new MemoryStream())
                    {
                        FileInfo fInfo = new FileInfo(name);
                        switch(fInfo.Extension)
                        {
                            case "png":
                            case ".png":
                                image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                                break;
                            default:
                                image.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                                break;
                        }

                        stream.Seek(0, SeekOrigin.Begin);

                        await stream.CopyToAsync(outBlob);
                    }

                }
            }
        }
    }
}
