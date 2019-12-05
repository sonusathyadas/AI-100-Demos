using ImageGallery.Models;
using ImageGallery.Utils;
using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ImageGallery.Controllers
{
    public class HomeController : Controller
    {
        private static BlobStorageHelper blobStorage;
        private static CosmosDBHelper cosmosDb;

        public async Task<ActionResult> Index()
        {
            try
            {
                await InitializeServices();
                var imagedata=cosmosDb.FindAllDocuments<ImageMetadata>().AsEnumerable();
                return View(imagedata);
            }catch(Exception ex)
            {
                return View();

            }
        }

        [HttpGet]
        public ActionResult Search()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Search(SearchModel model)
        {
            List<AzureSearchResultModel> searchResults = new List<AzureSearchResultModel>();
            try
            {
                var luisResult = await LuisServiceHelper.SearchIntent(model.SearchText);                
                var entities = luisResult.Entities.Where(e => e.Score > 0.6).Select(t => t).ToArray();
                foreach(var entity in entities)
                {
                    var searchResult = await SearchServiceHelper.SearchAsync(entity.Entity);
                    var docs = searchResult.Results.Select(s => s.Document).ToArray();
                    searchResults.AddRange(docs);
                    model.DetectedEntities.Add(entity.Entity);
                    model.SearchResults = searchResults;                                       
                }
                return View(model);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View(model);
            }
            
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        private async Task<bool> InitializeServices()
        {

            if (blobStorage == null || cosmosDb == null)
            {
                BlobStorageHelper.ContainerName = ConfigurationManager.AppSettings["ContainerName"];
                BlobStorageHelper.ConnectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
                blobStorage = await BlobStorageHelper.BuildAsync();
                CosmosDBHelper.AccessKey = ConfigurationManager.AppSettings["CosmosDBKey"];
                CosmosDBHelper.EndpointUri = ConfigurationManager.AppSettings["CosmosDBEndpointURI"];
                CosmosDBHelper.DatabaseName = ConfigurationManager.AppSettings["DatabaseName"];
                CosmosDBHelper.CollectionName = ConfigurationManager.AppSettings["CollectionName"];
                cosmosDb = await CosmosDBHelper.BuildAsync();
            }
            return true;

        }
    }

    
}