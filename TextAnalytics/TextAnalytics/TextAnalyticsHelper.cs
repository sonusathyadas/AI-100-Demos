using Azure;
using Azure.AI.TextAnalytics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TextAnalytics
{
    public class TextAnalyticsHelper
    {
        TextAnalyticsClient client;

        public TextAnalyticsHelper(string endpoint, string subscriptionKey)
        {
            client = new TextAnalyticsClient(new Uri(endpoint), new AzureKeyCredential(subscriptionKey));
        }

        public async Task<DocumentSentiment> AnalyzeSentitmentAsync(string text, string language="en")
        {
            DocumentSentiment result= await client.AnalyzeSentimentAsync(text, language);
            return result;
        }

        public async Task<DetectedLanguage> DetectLanguageAsync(string text, string country="US")
        {
            var result = await client.DetectLanguageAsync(text, country);
            return result.Value;
        }

        public async Task<CategorizedEntityCollection> RecognizeEntitiesAsync(string text, string language="en")
        {
            var result= await client.RecognizeEntitiesAsync(text, language);
            return result.Value;
        }

        public async Task<KeyPhraseCollection> ExtractKeyPhrasesAsync(string text, string language="en")
        {
            var result = await client.ExtractKeyPhrasesAsync(text, language);
            return result.Value;
        }
    }
}
