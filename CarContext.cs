using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.SqlServer;
namespace Cars
{
    public class CarContext : DbContext
    {
        public DbSet<Car> Cars { get; set; }  // apply LINQ operators against this DbSet()
        protected override void OnConfiguring(DbContextOptionsBuilder options) =>
            options
            .UseSqlServer("Data Source=(localdb)\\ProjectsV13; Initial Catalog=CarsDB")
            .EnableSensitiveDataLogging();
          
    }
}
