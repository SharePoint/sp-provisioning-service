//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.DomainModel
{
    public class ProvisioningAppDBContext: DbContext
    {
        public ProvisioningAppDBContext(): this(null)
        {
        }

        public ProvisioningAppDBContext(String connectionString = null) : 
            base(!String.IsNullOrEmpty(connectionString) ? connectionString : "PnPProvisioningAppDBContext")
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;

            // Database.SetInitializer(new ProvisioningAppDBContextInitializer());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Set the default schema to pnp
            modelBuilder.HasDefaultSchema("pnp");

            // Define common rule for Id property
            modelBuilder.Properties()
                .Where(p => p.Name == "Id")
                .Configure(p => p.IsKey());

            // And make the Id property identity if Int32/Guid
            modelBuilder.Properties()
                .Where(p => p.Name == "Id" && (p.PropertyType == typeof(Int32) || p.PropertyType == typeof(Guid)))
                .Configure(p => p.HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity));

            // Define the many-to-many relationship between Packages and Categories
            modelBuilder.Entity<Package>()
                .HasMany<Category>(p => p.Categories) // One Package can have many Categories
                .WithMany(c => c.Packages) // One Category can have many Packages
                    .Map(c => // Define the DB model matching objects' names
                    {
                        c.MapLeftKey("PackageId");
                        c.MapRightKey("CategoryId");
                        c.ToTable("PackagesCategories", "pnp");
                    });

            // Let the ProvisioningActionItem Id to be forced
            modelBuilder.Entity<ProvisioningActionItem>().Property(i => i.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Force an index on Tenant ID and Package ID for ProvisioningActionItems
            modelBuilder.Entity<ProvisioningActionItem>().HasIndex(i => new { i.TenantId, i.PackageId });
        }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Package> Packages { get; set; }

        public DbSet<Tenant> Tenants { get; set; }

        public DbSet<ContentPage> ContentPages { get; set; }

        public DbSet<NotificationRecipient> NotificationRecipients { get; set; }

        public DbSet<ProvisioningActionItem> ProvisioningActionItems { get; set; }

        public DbSet<ConsumerApp> ConsumerApps { get; set; }
    }
}
