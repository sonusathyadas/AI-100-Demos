using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Customvision_Demo
{
    class Program
    {
        private static string trainingKey = ConfigurationManager.AppSettings["CustomVision:Training:Key"];
        private static string trainingEndpoint = ConfigurationManager.AppSettings["CustomVision:Training:Endpoint"];

        private static string predictionKey = ConfigurationManager.AppSettings["CustomVision:Prediction:Key"];
        private static string predictionEndpoint = ConfigurationManager.AppSettings["CustomVision:Prediction:Endpoint"];

        private static CustomVisionTrainingClient trainingClient;
        private static CustomVisionPredictionClient predictionClient;

        static Project project;
        static List<Tag> imageTags;
        
        static void Main(string[] args)
        {
            bool isContinuing = true;

            Initialize();            
            do
            {
                Console.Clear();
                Console.WriteLine("1) Create project");
                Console.WriteLine("2) Load existing project");
                Console.WriteLine("3) Create tags in project");
                Console.WriteLine("4) Tag images");
                Console.WriteLine("5) Train and publish");
                Console.WriteLine("6) Predict image");
                Console.WriteLine("7) Exit");
                Console.Write("Your choice:");
                int ch = Convert.ToInt32(Console.ReadLine());
                switch (ch)
                {
                    case 1:
                        Console.WriteLine("Enter project name:");
                        string pjtName = Console.ReadLine();
                        project=CreateProjectAsync(pjtName).GetAwaiter().GetResult();
                        break;
                    case 2:
                        Console.WriteLine("Enter project name:");
                        string pName = Console.ReadLine();
                        project = GetProjectByNameAsync(pName).GetAwaiter().GetResult();
                        Console.WriteLine("Project loaded");                        
                        break;
                    case 3:
                        Console.WriteLine("Enter tag names seperated by comma:");
                        var tagNames = Console.ReadLine();
                        imageTags=CreateTagsAsync(tagNames.Split(',').ToList()).GetAwaiter().GetResult();
                        break;
                    case 4:
                        Console.WriteLine("Image directory path:");
                        var imageDir = Console.ReadLine();
                        Console.WriteLine("Enter tag names for images seperated by comma:");
                        var tagList =  Console.ReadLine().Split(',').ToList();
                        var tagGuids = GetTagGuids(tagList).GetAwaiter().GetResult();
                        TagImagesAsync(tagGuids, imageDir).GetAwaiter().GetResult();
                        break;
                    case 5:
                        var predictionResourceId = ConfigurationManager.AppSettings["CustomVision:Prediction:ResourceId"];
                        Console.WriteLine("Enter the publish model name:");
                        var publishName = Console.ReadLine();
                        TrainAndPublishAsync(publishName, predictionResourceId).GetAwaiter().GetResult();
                        break;
                    case 6:
                        Console.WriteLine("Enter image url/path to predict:");
                        var imagePath = Console.ReadLine();
                        var result=PredictImageAsync(imagePath).GetAwaiter().GetResult();
                        var imageData = JsonConvert.SerializeObject(result, Formatting.Indented);
                        Console.WriteLine(imageData);
                        break;
                    case 7:
                        isContinuing = false;
                        break;
                }
                Console.WriteLine("Press ENTER key to continue");
                Console.ReadLine();
            } while (isContinuing);

        }

        private static void Initialize()
        {
            trainingClient = new CustomVisionTrainingClient()
            {
                ApiKey = trainingKey,
                Endpoint = trainingEndpoint
            };
            predictionClient = new CustomVisionPredictionClient()
            {
                ApiKey=predictionKey,
                Endpoint=predictionEndpoint
            };
        }

        private static async Task<Project> CreateProjectAsync(string projectName)
        {
            //Create project
            Console.WriteLine($"Creating project :{projectName}");
            var pjt=await trainingClient.CreateProjectAsync(projectName);
            Console.WriteLine($"Project {projectName} created" );
            return pjt;
        }

        private static async Task<Project> GetProjectByNameAsync(string projectName)
        {
            var projects = await trainingClient.GetProjectsAsync();
            var pjt=projects.SingleOrDefault(p => p.Name == projectName);
            if(pjt==null)
                Console.WriteLine("Project not found");
            return pjt;
        }

        private static async Task<List<Tag>> CreateTagsAsync(IList<string> tagNames)
        {
            if(project==null)
            {
                Console.WriteLine("Project not created/retrieved yet");
                return null;
            }
            List<Tag> tags = new List<Tag>();
            foreach(var tag in tagNames)
            {
                Console.WriteLine($"Creating tag {tag}" );
                tags.Add(await trainingClient.CreateTagAsync(project.Id, tag));
            }
            Console.WriteLine("Tags created successfully");
            return tags; 
        }

        private static async Task<Task> TagImagesAsync(IList<Guid> tags, string imageDirectoryPath)
        {
            IList<string> images=LoadImageFromDisk(imageDirectoryPath);            
            foreach(var image in images)
            {
                Console.WriteLine($"Tagging image:{Path.GetFileName(image)}");
                using (var stream = new MemoryStream(File.ReadAllBytes(image)))
                {
                    await trainingClient.CreateImagesFromDataAsync(project.Id, stream, tags);
                }                
            }
            Console.WriteLine("Image tagging completed");
            return Task.CompletedTask;
        }

        private static async Task<bool?> TrainAndPublishAsync(string publishModelName, string predictionResourceId)
        {
            if(project==null)
            {
                Console.WriteLine("Project not loaded for training");
                return null;
            }
            Iteration iteration=null;
            try
            {
                Console.WriteLine("Start training project....");
                iteration = await trainingClient.TrainProjectAsync(project.Id);
                while (iteration.Status == "Training")
                {
                    Thread.Sleep(1000);
                }
                Console.WriteLine($"Training completed, Iteration name:{iteration.Name}, Id: {iteration.Id}");
            }catch(CustomVisionErrorException ex)
            {
                if (ex.Response.Content.Contains("BadRequestTrainingNotNeeded"))
                {
                    Console.WriteLine("Training not required...up to date");                    
                }                
                iteration = await GetLastPublishedIterationAsync();

            }
            try
            {
                Console.WriteLine($"Publishing project...");
                await trainingClient.PublishIterationAsync(project.Id, iteration.Id, publishModelName, predictionResourceId);
                Console.WriteLine($"Project published");
                return true;
            }
            catch(CustomVisionErrorException ex)
            {
                if (ex.Response.Content.Contains("BadRequestIterationIsPublished"))
                {
                    Console.WriteLine("This iteration is already published");
                }
                return false;
            }            
        }

        private static async Task<Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models.ImagePrediction> PredictImageAsync(string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                Console.WriteLine("Image not exits, invalid image path");
                return null;
            }
            var iteration = await GetLastPublishedIterationAsync();
            Stream testImage = File.OpenRead(imagePath);
            return await predictionClient.ClassifyImageAsync(project.Id, iteration.PublishName, testImage);
        }


        private static IList<string> LoadImageFromDisk(string imageDirectoryPath)
        {
            string[] extension = {".jpg", ".bmp", ".png", ".jpeg" };
            var files= Directory.GetFiles(imageDirectoryPath);
            return files.Where(img => extension.Contains(Path.GetExtension(img))).ToList();
        }

        private static async Task<List<Guid>> GetTagGuids(List<string> tags)
        {
            if(imageTags==null)
            {
                imageTags=await trainingClient.GetTagsAsync(project.Id) as List<Tag>;
            }
            List<Guid> guids = new List<Guid>();
            foreach(var tag in tags)
            {
                var temp=imageTags.Find(t => t.Name == tag);
                if (temp!=null)
                {
                    guids.Add(temp.Id);
                }
                else
                {
                    Console.WriteLine($"Tag {tag} not exists, creating it");
                    var tagCreated=await CreateTagsAsync(new List<string> { tag });
                    imageTags.AddRange(tagCreated);
                    guids.Add(tagCreated.First().Id);
                }
            }
            return guids;
        }

        private static async Task<Iteration> GetLastPublishedIterationAsync()
        {
            if (project == null)
            {
                Console.WriteLine("Project not loaded...");
                return null;
            }
            var iterations=await trainingClient.GetIterationsAsync(project.Id);
            return iterations.Last();
        }
    }
}
