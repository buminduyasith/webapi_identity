
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
        public DbSet<RefreshToken> RefreshToken { get; set; }

        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }


    }
}