using ImageGallery.Models;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ImageGallery.Utils
{
    public class SearchServiceHelper
    {
        public static async Task<DocumentSearchResult<AzureSearchResultModel>> SearchAsync(string searchKeyword)
        {
            try
            {
                string searchServiceName = ConfigurationManager.AppSettings["SearchServiceName"];
                string queryApiKey = ConfigurationManager.AppSettings["SearchServiceKey"];
                string indexName = ConfigurationManager.AppSettings["SearchServiceIndexName"];
                SearchParameters parameters;
                DocumentSearchResult<AzureSearchResultModel> results;

                SearchIndexClient indexClient = new SearchIndexClient(searchServiceName, indexName, new SearchCredentials(queryApiKey));
                parameters = new SearchParameters()
                {
                    Select = new[] { "id", "BlobUri", "Caption",  "Tags", "TaggedBy" }
                };
                results = await indexClient.Documents.SearchAsync<AzureSearchResultModel>(searchKeyword, parameters);

                return results;
            }catch(Exception ex)
            {
                return null;
            }
        }
    }
}