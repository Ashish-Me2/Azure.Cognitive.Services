using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OCR.Models
{
    [Serializable]
    public class LunchMenuModel
    {
            [JsonProperty("language")]
            public string Language { get; set; }
            [JsonProperty("orientation")]
            public string Orientation { get; set; }
            [JsonProperty("textAngle")]
            public double TextAngle { get; set; }
            [JsonProperty("regions")]
            public List<Region> Regions { get; set; }
        }
    [Serializable]
    public class Region
        {
            [JsonProperty("boundingBox")]
            public string BoundingBox { get; set; }
            [JsonProperty("lines")]
            public List<Line> Lines { get; set; }
        }
    [Serializable]
    public class Line
        {
            [JsonProperty("boundingBox")]
            public string BoundingBox { get; set; }
            [JsonProperty("words")]
            public List<Word> Words { get; set; }
        }
    [Serializable]
    public class Word
        {
            [JsonProperty("boundingBox")]
            public string BoundingBox { get; set; }
            [JsonProperty("text")]
            public string Text { get; set; }
        }
}


