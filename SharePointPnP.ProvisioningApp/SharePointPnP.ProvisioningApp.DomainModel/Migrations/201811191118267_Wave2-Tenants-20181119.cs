//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
namespace SharePointPnP.ProvisioningApp.DomainModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Wave2Tenants20181119 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "pnp.Tenants",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        TenantName = c.String(nullable: false, maxLength: 100),
                        ReferenceOwner = c.String(maxLength: 200),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("pnp.Tenants");
        }
    }
}
