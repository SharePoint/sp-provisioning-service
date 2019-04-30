//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
namespace SharePointPnP.ProvisioningApp.DomainModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Wave3NotificationRecipients : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "pnp.NotificationRecipients",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        DisplayName = c.String(nullable: false, maxLength: 200),
                        Email = c.String(nullable: false, maxLength: 200),
                        Enabled = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("pnp.NotificationRecipients");
        }
    }
}
