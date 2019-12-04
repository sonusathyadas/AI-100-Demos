using Microsoft.CognitiveServices.Speech;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechToTextFromMicrophoneDemo
{
    class Program
    {

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

            using (var recognizer = new SpeechRecognizer(config))
            {
                Console.WriteLine("Speak something which I can recognize...");

                while (true)
                {
                    var result = await recognizer.RecognizeOnceAsync();

                    if (result.Reason == ResultReason.RecognizedSpeech)
                    {
                        Console.WriteLine(result.Text);
                    }
                    else if (result.Reason == ResultReason.NoMatch)
                    {
                        Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                    }
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
    }
}
