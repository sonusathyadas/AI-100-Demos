using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace ComputerVisionDemo
{
    public class VisionHelper
    {
        private string subscriptionKey;
        private string visionEndPoint;
        private ComputerVisionClient visionClient;

        /// <summary>
        /// Set the Computer Vision service endpoint
        /// </summary>
        public string VisionEndPoint {
            set
            {
                visionEndPoint = value;
                ConfigureVisionClient();
            }
            private get { return visionEndPoint; }
        }

        /// <summary>
        /// Set the Computer Vision Subscription Key
        /// </summary>
        public string SubscriptionKey {
            private get { return subscriptionKey; }
            set {
                subscriptionKey = value;
                ConfigureVisionClient();
            }
        }

        /// <summary>
        /// Configure the ComputerVisionClient object using endpoint and subscription key
        /// </summary>
        private void ConfigureVisionClient()
        {
            if (!string.IsNullOrEmpty(subscriptionKey) && !string.IsNullOrEmpty(visionEndPoint))
            {
                var credentials = new ApiKeyServiceClientCredentials(subscriptionKey);
                visionClient = new ComputerVisionClient(credentials)
                {
                    Endpoint = visionEndPoint
                };
            }
        }
    
        /// <summary>
        /// Analyze the image from an http based image url
        /// </summary>
        /// <param name="imageUrl">Http url of image</param>
        /// <returns></returns>
        public async Task<ImageAnalysis> AnalyzeImageFromUrlAsync(string imageUrl)
        {
            if (!imageUrl.StartsWith("http"))
            {
                throw new Exception("Image must be a publicly accessible url. Provide an image url of http or https type");
            }
            List<VisualFeatureTypes> features = new List<VisualFeatureTypes>()
            {
                VisualFeatureTypes.Categories,
                VisualFeatureTypes.Brands,
                VisualFeatureTypes.Description,
                VisualFeatureTypes.Faces,
                VisualFeatureTypes.Tags,
                VisualFeatureTypes.Objects,
                VisualFeatureTypes.Adult
            };
            List<Details> details = new List<Details>()
            {
                Details.Celebrities,
                Details.Landmarks
            };
            ImageAnalysis result = await visionClient.AnalyzeImageAsync(imageUrl, features, details);
            return result;

        }

        /// <summary>
        /// Analyze the image from stream object
        /// </summary>
        /// <param name="stream">Stream object</param>
        /// <returns></returns>
        public async Task<ImageAnalysis> AnalyzeImageFromStreamAsync(Stream stream)
        {            
            List<VisualFeatureTypes> features = new List<VisualFeatureTypes>()
            {
                VisualFeatureTypes.Categories,
                VisualFeatureTypes.Brands,
                VisualFeatureTypes.Description,
                VisualFeatureTypes.Faces,
                VisualFeatureTypes.Tags,
                VisualFeatureTypes.Objects,
                VisualFeatureTypes.Adult
            };
            List<Details> details = new List<Details>()
            {
                Details.Celebrities,
                Details.Landmarks
            };
            ImageAnalysis result = await visionClient.AnalyzeImageInStreamAsync(stream, features, details);
            return result;

        }

        private async Task<ImageDescription> DescribeImageAsync(string imageUrl)
        {
            Stream stream = File.OpenRead(imageUrl);
            return await visionClient.DescribeImageInStreamAsync(stream);
        }

        private async Task<DetectResult> DetectObjectsAsync(string imageUrl)
        {
            Stream stream = File.OpenRead(imageUrl);
            return await visionClient.DetectObjectsInStreamAsync(stream);
        }

        private async Task<string> GenerateThumbnailAsync(string imageUrl)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var thumbnailFile = Path.Combine(desktopPath, $@"image_{DateTime.Now.Ticks.ToString()}.jpg");

            Stream stream = File.OpenRead(imageUrl);
            var imageStream = await visionClient.GenerateThumbnailInStreamAsync(200, 150, stream, smartCropping: true);
            FileStream fs = File.Create(thumbnailFile);
            imageStream.CopyTo(fs);
            fs.Flush();
            fs.Close();
            return Path.GetFullPath(thumbnailFile);

        }

        private async Task<OcrResult> DetectTextAsync(string imageUrl)
        {
            Stream stream = File.OpenRead(imageUrl);
            return await visionClient.RecognizePrintedTextInStreamAsync(true, stream);
        }
    }
}
