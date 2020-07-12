using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace CheckinLabs.Data.EF.SqlServer
{
    public class AppSqlServerContext : AppDbContext
    {
        public AppSqlServerContext(IConfiguration cfg) : base(cfg)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(DbConnectionString);
        }
    }

}
