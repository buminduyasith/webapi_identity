
using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using webapi_identity.Domains;



namespace webapi_identity.DataAccess
{
    public class TestDbContext : IdentityDbContext
    {
        public DbSet<Item> Items { get; set; }

        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        // {


        //     // Console.WriteLine(_configuration.GetConnectionString("SqlDatabase"));
        //     //_configuration.GetConnectionString("SqlDatabase"); //

        //     var connectionString = "Server=localhost; Database=CatalogDB; User Id=bumindu; Password=redgreen39";

        //     optionsBuilder.UseSqlServer(connectionString).EnableSensitiveDataLogging();



        // }

        // protected override void OnModelCreating(ModelBuilder modelBuilder)
        // {
        //     base.OnModelCreating(modelBuilder);
        //     // modelBuilder.Entity<Item>()
        //     // .Property(p=>p.Price)
        //     // .HasColumnType("decimal(18,4)");

        // }
    }
}