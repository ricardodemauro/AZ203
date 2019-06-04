using ConsoleAppTableStorage.Models;
using System;
using System.Threading.Tasks;

namespace ConsoleAppTableStorage
{
    class Program
    {
        static async Task Main(string[] args)
        {
            TableManager manager = new TableManager("customers");

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
