//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
namespace SharePointPnP.ProvisioningApp.DomainModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Wave2ContentPages20181120 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "pnp.ContentPages",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Content = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("pnp.Packages", "PropertiesMetadata", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("pnp.Packages", "PropertiesMetadata");
            DropTable("pnp.ContentPages");
        }
    }
}
