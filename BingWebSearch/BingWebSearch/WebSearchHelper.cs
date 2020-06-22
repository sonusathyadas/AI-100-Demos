using Microsoft.Azure.CognitiveServices.Search.WebSearch;
using Microsoft.Azure.CognitiveServices.Search.WebSearch.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BingWebSearch
{
    public class WebSearchHelper
    {
        private WebSearchClient searchClient;

        public WebSearchHelper(string subscriptionKey)
        {
            searchClient= new WebSearchClient(new ApiKeyServiceClientCredentials(subscriptionKey));

        }

        public async Task<SearchResponse> SearchWebAsync(string query)
        {            
            SearchResponse result = await searchClient.Web.SearchAsync(query, answerCount: 10, countryCode: "in");
            return result;
        }
    }
}
