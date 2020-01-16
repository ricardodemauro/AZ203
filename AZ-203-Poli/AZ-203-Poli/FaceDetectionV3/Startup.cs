using FaceDetectionV3.Infrastructure;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

[assembly: FunctionsStartup(typeof(FaceDetectionV3.Startup))]
namespace FaceDetectionV3
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddOptions();

            builder.Services.Configure<FaceOptions>(opt =>
            {
                opt.FaceSubscriptionKey = Environment.GetEnvironmentVariable("FACE_SUBSCRIPTION_KEY");
                opt.FaceEndpoint = Environment.GetEnvironmentVariable("FACE_ENDPOINT");
            });

            builder.Services.Configure<StorageOptions>(opt =>
            {
                opt.ContainerName = Environment.GetEnvironmentVariable("ContainerName");
                opt.StorageAccountName = Environment.GetEnvironmentVariable("StorageAccountName");
            });

            builder.Services.AddScoped<FaceApp>();
        }
    }
}
