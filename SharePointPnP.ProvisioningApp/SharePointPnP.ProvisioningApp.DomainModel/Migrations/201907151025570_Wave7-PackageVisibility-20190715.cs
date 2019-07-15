namespace SharePointPnP.ProvisioningApp.DomainModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Wave7PackageVisibility20190715 : DbMigration
    {
        public override void Up()
        {
            AddColumn("pnp.Packages", "Visible", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("pnp.Packages", "Visible");
        }
    }
}
