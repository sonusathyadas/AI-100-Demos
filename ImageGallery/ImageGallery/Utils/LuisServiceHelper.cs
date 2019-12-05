using ImageGallery.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace ImageGallery.Utils
{
    public class LuisServiceHelper
    {
        public static async Task<LuisResult> SearchIntent(string searchQuery)
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // This app ID is for a public sample app that recognizes requests to turn on and turn off lights
            var luisAppId = ConfigurationManager.AppSettings["LuisAppId"];
            var endpointKey = ConfigurationManager.AppSettings["LuisEndpointKey"];
            var endPoint = ConfigurationManager.AppSettings["LuisEndpoint"];
            // The request header contains your subscription key
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", endpointKey);

            // The "q" parameter contains the utterance to send to LUIS
            queryString["q"] = searchQuery;

            // These optional request parameters are set to their default values
            queryString["timezoneOffset"] = "0";
            queryString["verbose"] = "false";
            queryString["spellCheck"] = "false";
            queryString["staging"] = "false";

            var endpointUri = $"{endPoint}{luisAppId}?{queryString}";
            var response = await client.GetAsync(endpointUri);

            var strResponseContent = await response.Content.ReadAsStringAsync();
            var result=JsonConvert.DeserializeObject<LuisResult>(strResponseContent);
            return result;
        }
    }
}