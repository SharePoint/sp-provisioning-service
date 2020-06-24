//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
namespace SharePointPnP.ProvisioningApp.DomainModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Wave12ProvisioningPackagesId : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("pnp.PackagesCategories", "PackageId", "pnp.Packages");
            DropForeignKey("pnp.PackagesPlatforms", "PackageId", "pnp.Packages");
            DropPrimaryKey("pnp.Packages");
            AlterColumn("pnp.Packages", "Id", c => c.Guid(nullable: false));
            AddPrimaryKey("pnp.Packages", "Id");
            AddForeignKey("pnp.PackagesCategories", "PackageId", "pnp.Packages", "Id", cascadeDelete: true);
            AddForeignKey("pnp.PackagesPlatforms", "PackageId", "pnp.Packages", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("pnp.PackagesPlatforms", "PackageId", "pnp.Packages");
            DropForeignKey("pnp.PackagesCategories", "PackageId", "pnp.Packages");
            DropPrimaryKey("pnp.Packages");
            AlterColumn("pnp.Packages", "Id", c => c.Guid(nullable: false, identity: true));
            AddPrimaryKey("pnp.Packages", "Id");
            AddForeignKey("pnp.PackagesPlatforms", "PackageId", "pnp.Packages", "Id", cascadeDelete: true);
            AddForeignKey("pnp.PackagesCategories", "PackageId", "pnp.Packages", "Id", cascadeDelete: true);
        }
    }
}
