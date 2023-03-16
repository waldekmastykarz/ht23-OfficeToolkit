using System;
using System.Globalization;
using System.Text.Json.Serialization;

namespace HttpSample.Function.SharePoint.Models
{
    public class DownloadFileDetails
    {
        [JsonPropertyName("@odata.context")]
        public string DataContext { get; set; }

        [JsonPropertyName("@microsoft.graph.downloadUrl")]
        public string DownloadUrl { get; set; }

        public class File
        {
            [JsonPropertyName("mimeType")]
            public string MimeType { get; set; }

            [JsonPropertyName("hashes")]
            public Hashes Hashes { get; set; }
        }

        public class Hashes
        {
            public string quickXorHash { get; set; }
        }

    }
}
