using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImageGallery.Models
{
    public class ImageThumbnailModel
    {
        public string ImageUri { get; set; }
        public string TaggedBy { get; set; }
        public string[] Tags { get; set; }
        public string[] Categories { get; set; }
    }
}