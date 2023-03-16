using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;


namespace HttpSample.Function.SharePoint.Models
{
    public class FileRequestDto
    {
        [JsonProperty("sitePath")]
        public string SitePath { get; set; }

        [JsonProperty("siteId")]
        public string SiteId { get; set; }

        [JsonProperty("fileName")]
        public string FileName { get; set; }
    }
}
