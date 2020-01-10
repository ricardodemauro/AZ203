using ConsoleAppTableStorage.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace ConsoleAppTableStorage
{
    class Program
    {
        static readonly IConfiguration configuration;

        static Program()
        {
            configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .AddUserSecrets("ccaae0ba-923f-4824-9c39-a91f16045934")
                .Build();
        }

        static async Task Main(string[] args)
        {
            string connectionString = configuration["AzureStorageConnectionString"];
            TableManager manager = new TableManager("customers", connectionString);

            await manager.Add(new EmployeeSessionEntity(1, "ricardo", 29992));
            await manager.Add(new EmployeeSessionEntity(1, "ricardo2", 29992));
            await manager.Add(new EmployeeSessionEntity(1, "ricardo3", 29992));
            await manager.Add(new EmployeeSessionEntity(1, "ricardo4", 29992));

            Console.WriteLine("Hello World!");

            var result = await manager.GetAll();

            foreach (var item in result)
            {
                Console.WriteLine($"result item {item.Id}-{item.Name}-{item.Salaray}");
            }

            Console.ReadKey();
        }
    }
}
