using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.SqlServer;
using System;
using Microsoft.EntityFrameworkCore;

namespace ConsoleAppSQLData
{
    public class Program
    {
        private static readonly IServiceProvider serviceProvider;

        internal static readonly IConfiguration configuration;

        static Program()
        {
            configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var coll = new ServiceCollection();

            coll.AddDbContext<AppDbContext>(
                opts => opts.UseSqlServer(configuration.GetConnectionString("Default"))
            );

            serviceProvider = coll.BuildServiceProvider();
        }

        static void Main(string[] args)
        {
            var db = serviceProvider.GetService<AppDbContext>();
            db.Database.Migrate();

            for (int i = 0; i < 5; i++)
            {
                db.Customer.Add(new Models.Customer()
                {
                    Name = $"Sample - {Guid.NewGuid().ToString()}"
                });
            }

            db.SaveChanges();


            var customers = db.Customer.ToListAsync()
                .GetAwaiter().GetResult();

            foreach (var c in customers)
            {
                Console.WriteLine($"Id {c.Id} Name {c.Name}");
            }
            Console.WriteLine("Hello World!");
            Console.ReadKey();
        }
    }
}
