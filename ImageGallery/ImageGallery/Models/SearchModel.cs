using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImageGallery.Models
{
    public class SearchModel
    {
        private List<string> _detectedEntities = new List<string>();

        public string SearchText { get; set; }

        public List<AzureSearchResultModel> SearchResults { get; set; }
        
        public List<string> DetectedEntities {
            get { return _detectedEntities; }
            //set { _detectedEntities = value; }
        }
    }
}