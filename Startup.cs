using System;
using HttpSample.Common.Models;
using HttpSample.Common.Extensions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using HttpSample.Function.SharePoint.Extensions;

[assembly: FunctionsStartup(typeof(HttpSample.Startup))]
namespace HttpSample
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();
            builder.Services.SetCommonConfig();
            builder.Services.AddCommonDependencies();

            builder.Services.SetUpSharePointConfig();
            builder.Services.AddSharePointDependencies();
        }
    }
}
