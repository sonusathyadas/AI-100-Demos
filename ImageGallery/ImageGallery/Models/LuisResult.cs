using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImageGallery.Models
{
    public class LuisResult
    {
        [JsonProperty("query")]
        public string Query { get; set; }
        [JsonProperty("topScoringIntent")]
        public IntentScore TopScoringIntent { get; set; }
        [JsonProperty("entities")]
        public LuisEntity[] Entities { get; set; }
    }

    public class IntentScore
    {
        [JsonProperty("intent")]
        public string Intent { get; set; }
        [JsonProperty("score")]
        public double Score { get; set; }
    }
    public class LuisEntity
    {
        [JsonProperty("entity")]
        public string Entity { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("startIndex")]
        public int StartIndex { get; set; }
        [JsonProperty("endIndex")]
        public int EndIndex { get; set; }
        [JsonProperty("score")]
        public double Score { get; set; }
    }
}