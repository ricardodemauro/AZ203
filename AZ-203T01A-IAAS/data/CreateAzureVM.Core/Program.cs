using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace CreateAzureVM.Core
{
    class Program
    {
        static readonly IServiceProvider serviceProvider;

        static readonly ILogger<Program> logger;

        static Program()
        {
            var services = new ServiceCollection();

            services.AddScoped<DemoVirtualMachine>();
            services.AddLogging(c =>
            {
                c.AddConsole();
                c.AddDebug();
            });

            serviceProvider = services.BuildServiceProvider();

            logger = serviceProvider.GetService<ILogger<Program>>();
        }

        static async Task Main(string[] args)
        {
            logger.LogInformation("Comecando o programa");
            logger.LogInformation("Configurando arquivo azureauth.properties");

            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string path = Path.Combine(assemblyPath, "azureauth.properties");

            Environment.SetEnvironmentVariable("AZURE_AUTH_LOCATION", path, EnvironmentVariableTarget.Process);

            logger.LogInformation($"azureauth.properties path --> {path}");

            CancellationToken cancellationToken = CancellationToken.None;

            var vm = serviceProvider.GetService<DemoVirtualMachine>();

            vm.SetupCredentials();
            await vm.CreateResourceGroupAsync(cancellationToken);
            await vm.CreateAvailabilitySetAsync(cancellationToken);
            await vm.CreatePublicIPAddress(cancellationToken);
            await vm.CreateVirtualNetwork(cancellationToken);
            await vm.CreateNIC(cancellationToken);
            await vm.CreateVirtualMachine(cancellationToken);

            vm.Describe();

            Console.WriteLine("Concluído. Pressione qualquer tecla para terminar.");
            Console.ReadKey();
        }
    }
}
