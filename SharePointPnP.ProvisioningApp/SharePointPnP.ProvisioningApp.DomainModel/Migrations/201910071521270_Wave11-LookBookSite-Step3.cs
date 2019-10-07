namespace SharePointPnP.ProvisioningApp.DomainModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Wave11LookBookSiteStep3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("pnp.Categories", "Order", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("pnp.Categories", "Order");
        }
    }
}
