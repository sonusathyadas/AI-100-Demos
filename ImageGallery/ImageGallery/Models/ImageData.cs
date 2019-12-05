using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImageGallery.Models
{
    public class ImageData
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("image")]
        public dynamic Image { get; set; }
    }
}