using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CSharpWpfYouTube.Models
{    
    public class YouTubeVideoInfo
    {
        [JsonPropertyName("items")]
        public List<Item> ItemList { get; set; } = new List<Item>();
    }

    public class Item
    {
        [JsonPropertyName("snippet")]
        public Snippet? Snippet { get; set; }
    }

    public class Snippet
    {
        [JsonPropertyName("publishedAt")]
        public string? ReleaseDate { get; set; }
        [JsonPropertyName("title")]
        public string? Title { get; set; }
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        [JsonPropertyName("thumbnails")]
        public ThumbNails? ThumbNails { get; set; }
    }

    public class ThumbNails
    {
        [JsonPropertyName("default")]
        public ThumbNailData? Default { get; set; }
        [JsonPropertyName("medium")]
        public ThumbNailData? Medium { get; set; }
        [JsonPropertyName("high")]
        public ThumbNailData? High { get; set; }
        [JsonPropertyName("standard")]
        public ThumbNailData? Standard { get; set; }
        [JsonPropertyName("maxres")]
        public ThumbNailData? MaxRes { get; set; }
    }

    public class ThumbNailData
    {
        [JsonPropertyName("url")]
        public string? Url { get; set; }
        [JsonPropertyName("width")]
        public int Width { get; set; }
        [JsonPropertyName("height")]
        public int Height { get; set; }
    }
}
