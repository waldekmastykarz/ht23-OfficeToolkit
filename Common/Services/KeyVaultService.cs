using System;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Options;
using HttpSample.Common.Models;

namespace HttpSample.Common.Services
{
    public class KeyVaultService
    {
        private readonly SecretClient _secretClient;

        public KeyVaultService(IOptions<CommonConfig> commonConfigOptions, DefaultAzureCredential defaultAzureCredential)
        {
            _secretClient = new SecretClient(new Uri(commonConfigOptions.Value.KeyVaultUri), defaultAzureCredential);
        }

        public async Task<string> GetSecret(string secretName)
        {
            var secret = await _secretClient.GetSecretAsync(secretName);
            return secret.Value.Value;
        }
    }
}
