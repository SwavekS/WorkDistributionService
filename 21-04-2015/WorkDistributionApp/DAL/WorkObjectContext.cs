using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using WorkDistributionApp.Models;

namespace WorkDistributionApp.DAL
{
    public class WorkObjectContext : DbContext
    {
        public WorkObjectContext() : base("WorkObjectContext")
        {
        }

        public DbSet<WorkObject> WorkObjects { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Attachment> Attachments { get; set; }

        // helper table to keep stripe customer details
        public DbSet<StripeCustomers> StripeCustomers { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}