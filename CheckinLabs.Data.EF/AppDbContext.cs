using CheckinLabs.BL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace CheckinLabs.Data.EF
{
    public class AppDbContext : DbContext
    {
        public const string DbConnectionStringKey = "MainDb";
        protected string DbConnectionString { get; private set; }
        public AppDbContext(IConfiguration cfg) : base()
        {
            DbConnectionString = cfg.GetConnectionString(DbConnectionStringKey);
        }
        public Guid UID { get; } = Guid.NewGuid();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //modelBuilder.Properties().Where(p => p.Name == "Id").Configure(p => p.IsKey());
            //modelBuilder.Properties<string>().Configure(c => c.SetMaxLength(260));

            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<UserProfile>().ToTable("UserProfiles");
            modelBuilder.Entity<UserCheckin>().ToTable("UserCheckins");
            //modelBuilder.Entity<Branch>().Property(b => b.Addr).HasMaxLength(512);
            //modelBuilder.Entity<Branch>().Property(b => b.UrlOffset).HasMaxLength(1000);
            //modelBuilder.Entity<Publisher>(entity =>
            //{
            //    entity.HasKey(e => e.ID);
            //    entity.Property(e => e.Name).IsRequired();
            //});

            //modelBuilder.Entity<Book>(entity =>
            //{
            //    entity.HasKey(e => e.ISBN);
            //    entity.Property(e => e.Title).IsRequired();
            //    entity.HasOne(d => d.Publisher)
            //      .WithMany(p => p.Books);
            //});
        }
    }

}
