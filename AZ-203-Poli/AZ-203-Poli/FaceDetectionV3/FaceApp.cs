using FaceDetectionV3.Infrastructure;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FaceDetectionV3
{
    public class FaceApp
    {
        private readonly FaceOptions _faceOptions;

        private readonly ILogger<FaceApp> _logger;

        public FaceApp(IOptions<FaceOptions> faceOptions, ILogger<FaceApp> logger)
        {
            _faceOptions = faceOptions?.Value ?? throw new ArgumentNullException(nameof(faceOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /*
         *	AUTHENTICATE
         *	Uses subscription key and region to create a client.
         */
        private static IFaceClient Authenticate(string endpoint, string key)
        {
            return new FaceClient(new ApiKeyServiceClientCredentials(key)) { Endpoint = endpoint };
        }

        public Task<FaceRectangle> DetectFaceExtract(string url)
        {
            var client = Authenticate(_faceOptions.FaceEndpoint, _faceOptions.FaceSubscriptionKey);
            return DetectFaceExtract(client, url);
        }

        public Task<FaceRectangle> DetectFaceExtract(Stream stream)
        {
            var client = Authenticate(_faceOptions.FaceEndpoint, _faceOptions.FaceSubscriptionKey);
            return DetectFaceExtract(client, stream);
        }

        private async Task<FaceRectangle> DetectFaceExtract(IFaceClient client, string url)
        {
            _logger.LogInformation("Detecting face {FaceUrl}", url);

            IList<DetectedFace> detectedFaces;

            var faceAttributes = new List<FaceAttributeType> {
                FaceAttributeType.Accessories,
                FaceAttributeType.Age,
                FaceAttributeType.Blur,
                FaceAttributeType.Emotion,
                FaceAttributeType.Exposure,
                FaceAttributeType.FacialHair,
                FaceAttributeType.Gender,
                FaceAttributeType.Glasses,
                FaceAttributeType.Hair,
                FaceAttributeType.HeadPose,
                FaceAttributeType.Makeup,
                FaceAttributeType.Noise,
                FaceAttributeType.Occlusion,
                FaceAttributeType.Smile
            };

            // Detect faces with all attributes from image url.
            detectedFaces = await client.Face.DetectWithUrlAsync(
                    url,
                    returnFaceAttributes: faceAttributes,
                    recognitionModel: RecognitionModel.Recognition01);

            _logger.LogInformation("Face count {FaceCount} detected from image {imageFileName}", detectedFaces.Count, url);


            // Parse and print all attributes of each detected face.
            DetectedFace face = detectedFaces.Count > 0 ? detectedFaces[0] : null;

            if (face != null)
            {
                _logger.LogInformation("Face attributes for {FaceId}", face.FaceId ?? Guid.Empty);

                // Get bounding box of the faces
                _logger.LogInformation($"Rectangle(Left/Top/Width/Height) : {face.FaceRectangle.Left} {face.FaceRectangle.Top} {face.FaceRectangle.Width} {face.FaceRectangle.Height}");


                return face.FaceRectangle;
            }
            return new FaceRectangle();
        }

        private async Task<FaceRectangle> DetectFaceExtract(IFaceClient client, Stream stream)
        {
            _logger.LogInformation("Detecting face using stream");

            IList<DetectedFace> detectedFaces;

            var faceAttributes = new List<FaceAttributeType> {
                FaceAttributeType.Accessories,
                FaceAttributeType.Age,
                FaceAttributeType.Blur,
                FaceAttributeType.Emotion,
                FaceAttributeType.Exposure,
                FaceAttributeType.FacialHair,
                FaceAttributeType.Gender,
                FaceAttributeType.Glasses,
                FaceAttributeType.Hair,
                FaceAttributeType.HeadPose,
                FaceAttributeType.Makeup,
                FaceAttributeType.Noise,
                FaceAttributeType.Occlusion,
                FaceAttributeType.Smile
            };

            // Detect faces with all attributes from image url.
            detectedFaces = await client.Face.DetectWithStreamAsync(stream,
                    returnFaceAttributes: faceAttributes,
                    recognitionModel: RecognitionModel.Recognition01);

            _logger.LogInformation("Face count {FaceCount} detected from image using stream", detectedFaces.Count);


            // Parse and print all attributes of each detected face.
            DetectedFace face = detectedFaces.Count > 0 ? detectedFaces[0] : null;

            if (face != null)
            {
                _logger.LogInformation("Face attributes for {FaceId}", face.FaceId ?? Guid.Empty);

                // Get bounding box of the faces
                _logger.LogInformation($"Rectangle(Left/Top/Width/Height) : {face.FaceRectangle.Left} {face.FaceRectangle.Top} {face.FaceRectangle.Width} {face.FaceRectangle.Height}");


                return face.FaceRectangle;
            }
            return new FaceRectangle();
        }
    }
}
