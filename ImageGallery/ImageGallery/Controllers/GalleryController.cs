using ImageGallery.Models;
using ImageGallery.Utils;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.ProjectOxford.Vision;
using Microsoft.Cognitive.CustomVision;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.Cognitive.CustomVision.Models;

namespace ImageGallery.Controllers
{
    //[Authorize]
    public class GalleryController : Controller
    {

        private static BlobStorageHelper blobStorage;
        private static CosmosDBHelper cosmosDb;
        

        [HttpGet]
        public ActionResult TagImage()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> TagImage(HttpPostedFileBase file)
        {
            AnalysisResultViewModel model;

            if (ModelState.IsValid)
            {
                try
                {
                    if (Request.Files.Count > 0)
                    {
                        var Inputfile = Request.Files[0];

                        if (Inputfile != null && Inputfile.ContentLength > 0)
                        {
                            var filename = Path.GetFileName(Inputfile.FileName);
                            if (!Directory.Exists(Server.MapPath("~/uploadedfile/")))
                            {
                                Directory.CreateDirectory(Server.MapPath("~/uploadedfile/"));
                            }
                            if (System.IO.File.Exists(Path.Combine(Server.MapPath("~/uploadedfile/"), filename)))
                            {
                                System.IO.File.Delete(Path.Combine(Server.MapPath("~/uploadedfile/"), filename));
                            }
                            var path = Path.Combine(Server.MapPath("~/uploadedfile/"), filename);
                            Inputfile.SaveAs(path);

                            model = new AnalysisResultViewModel();
                            model.ImageLocalPath = "/uploadedfile/" + filename;
                            var analysisResult= await AnalyzeImage(path); //Call Computer vision to Tag images
                            //var predictionResult = await PredictImage(path); // Call custom vision
                            model.Insights = new ImageInsights
                            {
                                Caption= analysisResult.Caption,
                                Categories= analysisResult.Categories,
                                ImageId= analysisResult.ImageId,
                                Tags=analysisResult.Tags//.Union(predictionResult.Tags).ToArray()
                            };
                            ViewBag.FileStatus = "File uploaded successfully.";
                            return View(model);
                        }

                    }
                    else
                    {
                        ViewBag.FileStatus = "No files uploaded";
                    }

                }
                catch (Exception ex)
                {
                    ViewBag.FileStatus = "Error while file uploading."; ;
                }

            }
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> UpdateAnalysisData(AnalysisResultViewModel model)
        {
            try
            {
                await SaveData(model);
                ViewBag.Status =new UpdateStatusModel { Success=true, Message="Your image tag settings successfully saved!"};
                return View("TagImage");
            }
            catch (Exception ex)
            {
                ViewBag.Status = new UpdateStatusModel { Success = false, Message = "Failed to save image tag settings!" };
                return View("TagImage");
            }
        }

        [NonAction]
        private async Task<ImageInsights> AnalyzeImage(string imageLocalPath)
        {

            var fileName = Path.GetFileName(imageLocalPath);
            List<VisualFeature> features = new List<VisualFeature>()
            {
                VisualFeature.Categories, VisualFeature.Description,VisualFeature.Tags
            };
            try
            {
                VisionServiceHelper.ApiKey = ConfigurationManager.AppSettings["ComputerVisionKey"];
                VisionServiceHelper.EndPoint = ConfigurationManager.AppSettings["ComputerVisionEndPoint"];

                var resized = ImageResizer.ResizeIfRequired(imageLocalPath, 750);
                Func<Task<Stream>> imageCB = async () => System.IO.File.OpenRead(resized.Item2);
                var imageAnalysisResult = await VisionServiceHelper.AnalyzeImageAsync(imageCB, features);
                ImageInsights insights = new ImageInsights
                {
                    ImageId = fileName,
                    Caption = imageAnalysisResult.Description.Captions[0].Text,
                    Tags = imageAnalysisResult.Tags.Select(t => t.Name).ToArray(),
                    Categories = imageAnalysisResult.Categories.Where(t => t.Score > 0.60).Select(t => t.Name).ToArray()
                };
                return insights;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [NonAction]
        public async Task<ImageInsights> PredictImage(string imageLocalPath)
        {
            var fileName = Path.GetFileName(imageLocalPath);
            var customVisionProjectId = ConfigurationManager.AppSettings["CustomVisionProjectId"];
            var customVisionPredictionKey = ConfigurationManager.AppSettings["CustomVisionPredictionKey"];
            PredictionEndpointCredentials predictionEndpointCredentials = new PredictionEndpointCredentials(customVisionPredictionKey);
            PredictionEndpoint endpoint = new PredictionEndpoint(predictionEndpointCredentials);

            try
            {
                // Make a prediction against the project  
                var testImage = new MemoryStream(System.IO.File.ReadAllBytes(imageLocalPath));
                var result= await endpoint.PredictImageAsync(Guid.Parse(customVisionProjectId), testImage);
                
                ImageInsights insights = new ImageInsights
                {
                    ImageId = fileName,
                    Tags = result.Predictions.Where(s=>s.Probability >=0.60).Select(s=>s.Tag).ToArray()
                };
                return insights;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [NonAction]
        private async Task<ImageMetadata> SaveData(AnalysisResultViewModel data)
        {
            var fileName = Path.GetFileName(data.ImageLocalPath);
            Func<Task<Stream>> imageCB;

            try
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
                var existing = await cosmosDb.FindDocumentByIdAsync<ImageMetadata>(fileName);

                var resized = ImageResizer.ResizeIfRequired(Server.MapPath(data.ImageLocalPath), 750);
                imageCB = async () => System.IO.File.OpenRead(resized.Item2);
                var imageBlob = await blobStorage.UploadImageAsync(imageCB, fileName);                
                var metadata = new ImageMetadata(data.ImageLocalPath);
                metadata.AddInsights(data.Insights);
                metadata.BlobUri = imageBlob.Uri;
                metadata.TaggedBy = User.Identity.Name;
                if (existing == null)
                    metadata = (await cosmosDb.CreateDocumentIfNotExistsAsync(metadata, metadata.Id)).Item2;
                else
                    metadata = await cosmosDb.UpdateDocumentAsync(metadata, metadata.Id);

                return metadata;
            }
            catch (Exception ex)
            {
                return null;
            }
            
        }

    }
}