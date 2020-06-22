using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace BingWebSearch
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Search text/query:");
            string text = Console.ReadLine();
            WebSearchHelper searchHelper = new WebSearchHelper("1855f2e774264f0c8e48cbfb6e87be50");
            var result = await searchHelper.SearchWebAsync(text);
            if(result?.WebPages?.Value.Count > 0)
            {
                var resultText= JsonConvert.SerializeObject(result.WebPages.Value, Formatting.Indented);
                Console.WriteLine(resultText);
                Console.WriteLine("---------------------------------------------------");
            }

            if(result?.Images?.Value?.Count > 0)
            {
                var resultText = JsonConvert.SerializeObject(result.Images.Value, Formatting.Indented);
                Console.WriteLine(resultText);
                Console.WriteLine("---------------------------------------------------");
            }
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}
