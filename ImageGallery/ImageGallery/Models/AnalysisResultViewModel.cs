using ImageGallery.Utils;
using Microsoft.Cognitive.CustomVision.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ImageGallery.Models
{

    public class AnalysisResultViewModel
    {
        public string ImageLocalPath { get; set; }
        public ImageInsights Insights { get; set; }
        public ImagePredictionResultModel PredictionResult { get; set; }
    }
}