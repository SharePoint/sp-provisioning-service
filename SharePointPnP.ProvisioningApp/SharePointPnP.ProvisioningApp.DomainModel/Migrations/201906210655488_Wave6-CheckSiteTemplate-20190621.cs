namespace SharePointPnP.ProvisioningApp.DomainModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Wave6CheckSiteTemplate20190621 : DbMigration
    {
        public override void Up()
        {
            AddColumn("pnp.Packages", "MatchingSiteBaseTemplateId", c => c.String());
            AddColumn("pnp.Packages", "ForceNewSite", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("pnp.Packages", "ForceNewSite");
            DropColumn("pnp.Packages", "MatchingSiteBaseTemplateId");
        }
    }
}
