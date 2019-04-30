//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
namespace SharePointPnP.ProvisioningApp.DomainModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Wave220181116 : DbMigration
    {
        public override void Up()
        {
            AddColumn("pnp.Packages", "Promoted", c => c.Boolean(nullable: false, defaultValue: false));
            AddColumn("pnp.Packages", "Preview", c => c.Boolean(nullable: false, defaultValue: false));
            AddColumn("pnp.Packages", "TimesApplied", c => c.Long(nullable: false, defaultValue: 0));
        }
        
        public override void Down()
        {
            DropColumn("pnp.Packages", "TimesApplied");
            DropColumn("pnp.Packages", "Preview");
            DropColumn("pnp.Packages", "Promoted");
        }
    }
}
