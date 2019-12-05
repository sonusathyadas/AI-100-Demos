using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Spatial;
using Newtonsoft.Json;

namespace ImageGallery.Models
{
    public class AzureSearchResultModel
    {
        [System.ComponentModel.DataAnnotations.Key]
        [JsonProperty("id")]
        public string Id { get; set; }

        public string BlobUri { get; set; }

        [IsSearchable, IsSortable]
        public string Caption { get; set; }

        [IsSearchable, IsFacetable, IsFilterable, IsSortable]
        public string[] Categories { get; set; }

        [IsSearchable, IsFacetable, IsFilterable]
        public string[] Tags { get; set; }

        [IsSearchable, IsFilterable]
        public string TaggedBy { get; set; }
    }
}