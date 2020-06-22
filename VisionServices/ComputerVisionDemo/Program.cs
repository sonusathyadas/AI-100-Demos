using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace ComputerVisionDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            VisionHelper helper = new VisionHelper();
            helper.VisionEndPoint = "https://ai100-computervision.cognitiveservices.azure.com/";
            helper.SubscriptionKey = "66dffa77680f481faa9225ac64fe0e30";

            Console.WriteLine("1) Analyze image from Url");
            Console.WriteLine("2) Analyze local image");
            var ch = Int32.Parse(Console.ReadLine());
            if (ch == 1)
            {
                Console.WriteLine("Image Url (http or https):");
                string url = Console.ReadLine();
                var result=await helper.AnalyzeImageFromUrlAsync(url);
                PrintAsJson(result);
            }else if (ch == 2)
            {
                Console.WriteLine("Local image path:");
                string filePath = Console.ReadLine();
                FileStream fs = File.OpenRead(filePath);
                var result = await helper.AnalyzeImageFromStreamAsync(fs);
                fs.Close();
                PrintAsJson(result);
            }
            else
            {
                Console.WriteLine("Invalid choice");
            }
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static void PrintAsJson(object data)
        {
            var text = JsonConvert.SerializeObject(data,Formatting.Indented);
            Console.WriteLine(text);
        }
    }
}
