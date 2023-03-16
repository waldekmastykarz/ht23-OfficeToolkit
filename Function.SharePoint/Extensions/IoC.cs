using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HttpSample.Function.SharePoint.Models;
using HttpSample.Function.SharePoint.Services;

namespace HttpSample.Function.SharePoint.Extensions
{
    public static class IoC
    {
        public static void AddSharePointDependencies(this IServiceCollection service)
        {
            service.AddScoped<ISharePointClient, SharePointClient>();
        }

        public static void SetUpSharePointConfig(this IServiceCollection service)
        {
            service.AddOptions<SharePointConfig>().Configure<IConfiguration>((settings, configuration) => configuration.GetSection("CwdToolBox:SharePoint:ApiSettings").Bind(settings));
            service.AddOptions<SharePointStorageConfig>().Configure<IConfiguration>((settings, configuration) => configuration.GetSection("AzureWebJobStorage").Bind(settings));
        }
    }
}
