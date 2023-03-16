using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.Graph;
using Azure.Identity;
using Azure.Storage.Blobs;
using HttpSample.Common.Extensions;
using HttpSample.Function.SharePoint.Models;
using HttpSample.Function.SharePoint.Services;

namespace HttpSample.Function.SharePoint
{
    public class SharePointFunctions
    {
        private readonly ISharePointClient _sharePointClient;
        private readonly SharePointStorageConfig _storageConfig;
        private readonly DefaultAzureCredential _defaultAzureCredential;

        private string ContainerNameForBlobs = "business-automation";
        private string BaseFolderForBlobs = "to_be_processed/";

        public SharePointFunctions(DefaultAzureCredential defaultAzureCredential, IOptions<SharePointStorageConfig> storageConfigOption, ISharePointClient sharePointClient)
        {
            _sharePointClient = sharePointClient;
            _defaultAzureCredential = defaultAzureCredential;
            _storageConfig = storageConfigOption.Value;
        }


        [FunctionName(nameof(SharePointDownload))]
        [OpenApiOperation(operationId: nameof(SharePointDownload), "SharePoint Online", Description = "ApI for finding and downloading files from Sharepoint Online")]
        [OpenApiRequestBody("application/json", typeof(FileRequestDto), Required = true)]
        [OpenApiParameter(name: "calling-application", In = ParameterLocation.Header, Type = typeof(string), Required = true)]
        [OpenApiParameter(name: "calling-application-owner", In = ParameterLocation.Header, Type = typeof(string), Required = true)]
        [OpenApiParameter(name: "correlation-id", In = ParameterLocation.Header, Type = typeof(string), Required = false)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "plain/text", bodyType: typeof(string))]
        public async Task<IActionResult> SharePointDownload(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "sharepoint/download")] HttpRequest request,
            ILogger logger)
        {

            //Recieve a Post Request from a logic app that pulls file attributes from Sharepoint when files are uploaded
            var correlationId = "UNKNOWN";
            try
            {
                correlationId = request.GetFromRequestOrNewCorrelationId(logger);
                using var _ = logger.BeginScope("{correlationId} - ", correlationId);
                request.ValidateHeaders(logger, correlationId);

                var requestObj = await request.GetBodyAsync<FileRequestDto>();
                var sitePath = requestObj.SitePath;
                var newPath = sitePath.Substring(sitePath.IndexOf('/'));

                // Requesting Microsoft Graph Client Service
                logger.LogInformation("Authenticating Graph Service Client with KeyVault Secrets");
                var graphClient = await _sharePointClient.GetGraphAPIClient();
                logger.LogInformation("Done Authenticating Graph Service Client");

                var requestUrl = graphClient.Sites[$"{requestObj.SiteId}"].Drive.Root.ItemWithPath(newPath + requestObj.FileName).RequestUrl;

                logger.LogInformation("Starting API Request to Sharepoint URL: {requestUrl}", requestUrl);
                var fileRequest = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                await graphClient.AuthenticationProvider.AuthenticateRequestAsync(fileRequest);
                var response = await graphClient.HttpProvider.SendAsync(fileRequest);
                var streamTask = await response.Content.ReadAsStreamAsync();

                var fileDetails = await JsonSerializer.DeserializeAsync<DownloadFileDetails>(streamTask);
                logger.LogInformation("Done retrieving file information...");

                logger.LogInformation("Start downloading {fileName} from SharePoint Online Folder: {SitePath}", requestObj.FileName, newPath);
                var downloadRequest = new HttpRequestMessage(HttpMethod.Post, fileDetails.DownloadUrl);
                await graphClient.AuthenticationProvider.AuthenticateRequestAsync(downloadRequest);
                var dresponse = await graphClient.HttpProvider.SendAsync(downloadRequest);
                var bytesContent = await dresponse.Content.ReadAsStreamAsync();
                logger.LogInformation("Done downloading file...");

                //return new FileContentResult(bytesContent, fileDetails.File.MimeType);

                // Upload content to blob
                logger.LogInformation("Uploading file to storage container");
                var storageUrl = _storageConfig.blobServiceUri;
                var blobServiceClient = new BlobServiceClient(new Uri(storageUrl), _defaultAzureCredential);

                var blobContainerClient = blobServiceClient.GetBlobContainerClient(ContainerNameForBlobs);
                var uploadResponse = await blobContainerClient.UploadBlobAsync(BaseFolderForBlobs + requestObj.FileName, bytesContent);
                var rawResponse = uploadResponse.GetRawResponse();

                if (rawResponse.IsError)
                {
                    logger.LogError("Error: failed to upload {FileName} to {Blob}! {ErrorReasonPhrase}, {ErrorReasonCode}", requestObj.FileName, blobContainerClient.Name, rawResponse.ReasonPhrase, rawResponse.Status);
                }

                logger.LogInformation("Successfully uploaded blob to container...");
            }
            catch (Exception ex)
            {
                var error = new Exception($"Failed to process file from SharePoint Online. ErrorMessage: {ex.Message}");
                logger.LogError(ex, "{correlationId} - Failed to process file from SharePoint Online", correlationId);
                return new BadRequestObjectResult(error.Message);
            } 

            return new OkObjectResult($"Successful Operation, CorrelationId: {correlationId}");
        }
    }
}
