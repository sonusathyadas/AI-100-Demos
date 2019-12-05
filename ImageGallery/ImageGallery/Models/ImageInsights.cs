using Microsoft.ProjectOxford.Vision.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImageGallery.Models
{
    public class ImageInsights
    { 
        public string ImageId { get; set; }
        public string Caption { get; set; }
        public string[] Tags { get; set; }
        public string[] Categories { get; set; }
    }
}