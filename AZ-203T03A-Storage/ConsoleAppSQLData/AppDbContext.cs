using ConsoleAppSQLData.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleAppSQLData
{
    public class AppDbContext : DbContext
    {
        public DbSet<Customer> Customer { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {

        }
    }
}
