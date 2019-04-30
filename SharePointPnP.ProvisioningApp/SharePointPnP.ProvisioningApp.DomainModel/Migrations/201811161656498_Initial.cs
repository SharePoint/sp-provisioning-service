//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
namespace SharePointPnP.ProvisioningApp.DomainModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "pnp.Categories",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        DisplayName = c.String(nullable: false, maxLength: 200),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "pnp.Packages",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Author = c.String(maxLength: 200),
                        AuthorLink = c.String(maxLength: 1000),
                        Version = c.String(nullable: false, maxLength: 50),
                        DisplayName = c.String(nullable: false, maxLength: 200),
                        Description = c.String(nullable: false),
                        ImagePreviewUrl = c.String(nullable: false, maxLength: 400),
                        PackageType = c.Int(nullable: false),
                        PackageUrl = c.String(nullable: false, maxLength: 400),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "pnp.PackagesCategories",
                c => new
                    {
                        PackageId = c.Guid(nullable: false),
                        CategoryId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.PackageId, t.CategoryId })
                .ForeignKey("pnp.Packages", t => t.PackageId, cascadeDelete: true)
                .ForeignKey("pnp.Categories", t => t.CategoryId, cascadeDelete: true)
                .Index(t => t.PackageId)
                .Index(t => t.CategoryId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("pnp.PackagesCategories", "CategoryId", "pnp.Categories");
            DropForeignKey("pnp.PackagesCategories", "PackageId", "pnp.Packages");
            DropIndex("pnp.PackagesCategories", new[] { "CategoryId" });
            DropIndex("pnp.PackagesCategories", new[] { "PackageId" });
            DropTable("pnp.PackagesCategories");
            DropTable("pnp.Packages");
            DropTable("pnp.Categories");
        }
    }
}
