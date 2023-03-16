using System;
using Azure.Identity;
using HttpSample.Common.Models;
using HttpSample.Common.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HttpSample.Common.Extensions
{
    public static class IoC
    {
        public static void AddCommonDependencies(this IServiceCollection service)
        {
            service.AddSingleton<DefaultAzureCredential>();
            service.AddSingleton<KeyVaultService>();

            // Add Default Azure Creds and KeyVault Service as scoped
            //service.AddScoped(sp =>
            //{
            //    var commonConfigOptions = sp.GetRequiredService<IOptions<CommonConfig>>();
            //    var defaultAzureCreds = sp.GetRequiredService<DefaultAzureCredential>();

            //    var client = new SecretClient(new Uri(commonConfigOptions.Value.KeyVaultUri), defaultAzureCreds);
            //    return client;
            //});

        }

        public static void SetCommonConfig(this IServiceCollection service)
        {
            service.AddOptions<CommonConfig>().Configure<IConfiguration>((settings, configuration) => configuration.GetSection("CwdToolBox").Bind(settings));
        }
    }
}
