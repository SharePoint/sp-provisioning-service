//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
namespace SharePointPnP.ProvisioningApp.DomainModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Wave12ForceExistingSite : DbMigration
    {
        public override void Up()
        {
            AddColumn("pnp.Packages", "ForceExistingSite", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("pnp.Packages", "ForceExistingSite");
        }
    }
}
