using exercise.wwwapi.Configuration;
using exercise.wwwapi.Models;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace exercise.wwwapi.Data
{
    public class DatabaseContext : DbContext
    {
        private string _connectionString;
        public DatabaseContext()
        {
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            _connectionString = configuration.GetValue<string>("ConnectionStrings:DefaultConnectionString")!;
            

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(databaseName: "Database");
        }
   
        public DbSet<User> Users { get; set; }
    }
}
