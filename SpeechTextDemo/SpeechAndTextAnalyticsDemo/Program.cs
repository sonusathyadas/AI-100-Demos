using Microsoft.CognitiveServices.Speech;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Rest;
using System.Net.Http;
using System.Threading;

namespace SpeechAndTextAnalyticsDemo
{
    class Program
    {
        static TextAnalyticsClient client;

        static void Main(string[] args)
        {
            RecognizeSpeechAsync().Wait();
            Console.ReadLine(); 
        }
        public static async Task RecognizeSpeechAsync()
        {
            var speechKey = ConfigurationManager.AppSettings["Speech:Key"];
            var speechLocation= ConfigurationManager.AppSettings["Speech:Location"];
            var config = SpeechConfig.FromSubscription(speechKey, speechLocation);            
            var key = ConfigurationManager.AppSettings["TextAnalytics:Key"];
            var  endpoint = ConfigurationManager.AppSettings["TextAnalytics:Endpoint"];
            Initialize(key, endpoint);

            using (var recognizer = new SpeechRecognizer(config))
            {
                Console.WriteLine("Speak something which I can recognize...");

                while (true)
                {
                    var result = await recognizer.RecognizeOnceAsync();

                    if (result.Reason == ResultReason.RecognizedSpeech)
                    {
                        var sentimentResult = client.Sentiment(result.Text, "en");
                        Console.WriteLine(result.Text);
                        if(sentimentResult.Score >= 0.5)
                        {
                            Console.WriteLine("I think you are in a happy mood");
                        }
                        else
                        {
                            Console.WriteLine("I am sorry, I think you are in a sad mood");
                        }                        
                    }
                    //else if (result.Reason == ResultReason.NoMatch)
                    //{
                    //    Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                    //}
                    else if (result.Reason == ResultReason.Canceled)
                    {
                        var cancellation = CancellationDetails.FromResult(result);
                        Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                        if (cancellation.Reason == CancellationReason.Error)
                        {
                            Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                            Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                            Console.WriteLine($"CANCELED: Did you update the subscription info?");
                        }
                    }
                }
            }
        }
        private static void Initialize(string key, string endpoint)
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
