using Microsoft.EntityFrameworkCore;
using MobileSync.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSync.Server
{
    public class RemoteCustomerContext : DbContext
    {
        public RemoteCustomerContext() : base()
        {
            Database.EnsureCreated();
        }

        public DbSet<RemoteCustomer> Customers { get; set; }
        public DbSet<RemoteCustomerItemHistory> CustomerItemHistory { get; set; }

        string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings[nameof(RemoteCustomerContext)].ConnectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(GetConnectionString());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RemoteCustomer>().ToTable("Customers");
            modelBuilder.Entity<RemoteCustomerItemHistory>().ToTable("CustomerItemHistory");
        }
    }

    public class RemoteCustomer
    {
        public int Id { get; set; }

        public int VersionNumber { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public DateTime LastUpdateDateTime { get; set; }

        public DateTime DeletedDateTime { get; set; }

        public bool IsDeleted { get; set; }

        public string Name { get; set; }

        public string Company { get; set; }

        public string Title { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Notes { get; set; }
    }

    public class RemoteCustomerItemHistory : ItemUpdateHistory
    {

    }
}
