using ImageGallery.Models;
using ImageGallery.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ImageGallery.Controllers
{
    public class UserController : Controller
    {
        // GET: User
        public ActionResult Face()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> UploadImage(ImageData[] imageData)
        {

            try
            {
                var imagePaths = SaveImages(imageData);
                FaceRecognizerHelper.FaceApiKey = ConfigurationManager.AppSettings["FaceApiKey"];
                FaceRecognizerHelper.FaceApiEndpoint = ConfigurationManager.AppSettings["FaceApiEndpoint"];
                FaceRecognizerHelper faceHelper = FaceRecognizerHelper.InitializeService();
                await faceHelper.TainImagesAsync(User.Identity.Name, imagePaths);
                return Json(true);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, "Unable to save image.");
            }
        }

        [NonAction]
        private string[] SaveImages(ImageData[] imageData)
        {
            List<string> imagePaths = new List<string>();
            string username = User.Identity.Name;
            string path = Server.MapPath("~/faceimages");
            foreach (var item in imageData)
            {
                string fileNameWitPath = $"{path}/{username}_{item.Id}.png";
                imagePaths.Add(fileNameWitPath);
                using (FileStream fs = new FileStream(fileNameWitPath, FileMode.Create))
                {
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {
                        byte[] data = Convert.FromBase64String(item.Image);//convert from base64
                        bw.Write(data);
                        bw.Close();
                    }
                }
            }
            return imagePaths.ToArray();
        }

    }
}