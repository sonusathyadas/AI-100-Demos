using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerVision_Demo
{
    class Program
    {
        
        private static ComputerVisionClient visionClient { get; set; }
        private static List<VisualFeatureTypes> features = new List<VisualFeatureTypes>()
            {
              VisualFeatureTypes.Categories, VisualFeatureTypes.Description,
              VisualFeatureTypes.Faces, VisualFeatureTypes.ImageType,
              VisualFeatureTypes.Tags, VisualFeatureTypes.Adult,
              VisualFeatureTypes.Color, VisualFeatureTypes.Brands,
              VisualFeatureTypes.Objects
            };

        static void Main(string[] args)
        {
            InitializeVisionService();
            var imagePath = @"Your_image_path";
            bool isContinuing = true;
            do
            {
                Console.Clear();
                Console.WriteLine("1) Analyze Image");
                Console.WriteLine("2) Describe Image");
                Console.WriteLine("3) Detect Objects");
                Console.WriteLine("4) Generate thumbnail");
                Console.WriteLine("5) Detect text");
                Console.WriteLine("6) Exit");

                Console.WriteLine("Your choice:");
                int ch = Convert.ToInt32(Console.ReadLine());
                dynamic result=null;
                switch (ch)
                {
                    case 1:
                        result = AnalyzeImageAsync(imagePath, features).GetAwaiter().GetResult();
                        break;
                    case 2:
                        result = DescribeImageAsync(imagePath).GetAwaiter().GetResult();
                        break;
                    case 3:
                        result = DetectObjectsAsync(imagePath).GetAwaiter().GetResult();
                        break;
                    case 4:
                        var thumbnailPath = GenerateThumbnailAsync(imagePath).GetAwaiter().GetResult();
                        result = new { ThumbnailPath = thumbnailPath };
                        break;
                    case 5:
                        result = DetectTextAsync(imagePath).GetAwaiter().GetResult();
                        break;
                    case 6:
                        isContinuing = false;
                        break;                        
                }
                if (isContinuing && result!=null)
                {
                    var resultText = JsonConvert.SerializeObject(result, Formatting.Indented);
                    Console.WriteLine(resultText);
                }
                Console.WriteLine("Press ENTER key to continue");
                Console.ReadLine();
            } while (isContinuing);            
        }

        private static void InitializeVisionService()
        {
            var endpoint = ConfigurationManager.AppSettings["Vision:Endpoint"];
            var key = ConfigurationManager.AppSettings["Vision:Key"];
            visionClient = new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
            { Endpoint = endpoint };
        }

        private static async Task<ImageAnalysis> AnalyzeImageAsync(string imageUrl, IList<VisualFeatureTypes> features)
        {
            Stream stream=File.OpenRead(imageUrl);
            return await visionClient.AnalyzeImageInStreamAsync(stream, features);
        }

        private static async Task<ImageDescription> DescribeImageAsync(string imageUrl)
        {
            Stream stream = File.OpenRead(imageUrl);
            return await visionClient.DescribeImageInStreamAsync(stream);
        }

        private static async Task<DetectResult> DetectObjectsAsync(string imageUrl)
        {
            Stream stream = File.OpenRead(imageUrl);
            return await visionClient.DetectObjectsInStreamAsync(stream);            
        }

        private static async Task<string> GenerateThumbnailAsync(string imageUrl)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var thumbnailFile = Path.Combine(desktopPath, $@"image_{DateTime.Now.Ticks.ToString()}.jpg");

            Stream stream = File.OpenRead(imageUrl);
            var imageStream=await visionClient.GenerateThumbnailInStreamAsync(200, 150, stream, smartCropping: true);
            FileStream fs =  File.Create(thumbnailFile);
            imageStream.CopyTo(fs);
            fs.Flush();
            fs.Close();
            return Path.GetFullPath(thumbnailFile);
                        
        }

        private static async Task<OcrResult> DetectTextAsync(string imageUrl)
        {
            Stream stream = File.OpenRead(imageUrl);
            return await visionClient.RecognizePrintedTextInStreamAsync(true,stream);            
        }
    }
}
