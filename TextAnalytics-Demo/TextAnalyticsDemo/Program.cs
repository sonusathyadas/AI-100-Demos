using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Rest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TextAnalyticsDemo
{
    class Program
    {
        static TextAnalyticsClient client;
        static string key = ConfigurationManager.AppSettings["TextAnalytics:Key"];
        static string endpoint = ConfigurationManager.AppSettings["TextAnalytics:Endpoint"];

        static void Main(string[] args)
        {
            
            Initialize();

            Console.WriteLine("Type your statement:");
            var statement = Console.ReadLine();
            var sentimentResult = client.Sentiment(statement, "en");
            var sentimentResultText= JsonConvert.SerializeObject(sentimentResult, Formatting.Indented);
            Console.WriteLine(sentimentResultText);

            var langResult = client.DetectLanguage(statement,"US");
            var langResultText = JsonConvert.SerializeObject(langResult, Formatting.Indented);
            Console.WriteLine(langResultText);

            var entitiesResult = client.Entities(statement);
            var entitiesResultText = JsonConvert.SerializeObject(entitiesResult, Formatting.Indented);
            Console.WriteLine(entitiesResultText);            

            Console.ReadLine();
        }

        private static void Initialize()
        {
            ApiKeyServiceClientCredentials credentials = new ApiKeyServiceClientCredentials(key);

            client = new TextAnalyticsClient(credentials)
            {
                Endpoint = endpoint
            };
        }
    }

    class ApiKeyServiceClientCredentials : ServiceClientCredentials
    {
        private readonly string apiKey;

        public ApiKeyServiceClientCredentials(string apiKey)
        {
            this.apiKey = apiKey;
        }

        public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            request.Headers.Add("Ocp-Apim-Subscription-Key", this.apiKey);
            return base.ProcessHttpRequestAsync(request, cancellationToken);
        }
    }
}
