using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppTableStorage.Models
{
    public class EmployeeSessionEntity : TableEntity
    {
        public EmployeeSessionEntity()
        {
        }
        public EmployeeSessionEntity(int id, string name, double sal)
        {
            Id = id;
            Name = name;
            Salaray = sal;
            PartitionKey = name;
            RowKey = Guid.NewGuid().ToString();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public double Salaray { get; set; }
    }
}
