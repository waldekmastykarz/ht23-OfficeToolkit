namespace HttpSample.Function.SharePoint.Models
{
    public class SharePointStorageConfig
    {
        public string blobServiceUri { get; set; }
    }

    public class SharePointConfig
    {
        public string KeyvaultSecret_TenantId { get; set; }

        public string KeyvaultSecret_ClientId { get; set; }

        public string KeyvaultSecret_ClientSecret { get; set; }
    }
}
