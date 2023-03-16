using System;
using System.Threading.Tasks;
using Microsoft.Graph;
using Azure.Identity;
using Microsoft.Extensions.Options;
using HttpSample.Common.Services;
using HttpSample.Function.SharePoint.Models;

namespace HttpSample.Function.SharePoint.Services
{
    public class SharePointClient : ISharePointClient
    {
        private readonly KeyVaultService _keyVaultService;
        private readonly SharePointConfig _config;

        public SharePointClient(IOptions<SharePointConfig> options, KeyVaultService keyVaultService)
        {
            _keyVaultService = keyVaultService;
            _config = options.Value;
        }

        public async Task<GraphServiceClient> GetGraphAPIClient()
        {
            var scopes = new[] { "https://graph.microsoft.com/.default" };

            var tenantId = await _keyVaultService.GetSecret(_config.KeyvaultSecret_TenantId);
            var clientId = await _keyVaultService.GetSecret(_config.KeyvaultSecret_ClientId);
            var clientSecret = await _keyVaultService.GetSecret(_config.KeyvaultSecret_ClientSecret);

            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret, options);

            var graphClient = new GraphServiceClient(clientSecretCredential, scopes);

            return graphClient;
        }
    }
}
