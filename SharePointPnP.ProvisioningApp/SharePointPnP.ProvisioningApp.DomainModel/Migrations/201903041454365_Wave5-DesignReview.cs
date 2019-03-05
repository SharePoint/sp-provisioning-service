namespace SharePointPnP.ProvisioningApp.DomainModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Wave5DesignReview : DbMigration
    {
        public override void Up()
        {
            AddColumn("pnp.Packages", "Abstract", c => c.String(nullable: false));
            AddColumn("pnp.Packages", "SortOrder", c => c.Int(nullable: false));
            AddColumn("pnp.Packages", "SortOrderPromoted", c => c.Int(nullable: false));
            AddColumn("pnp.Packages", "RepositoryRelativeUrl", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("pnp.Packages", "RepositoryRelativeUrl");
            DropColumn("pnp.Packages", "SortOrderPromoted");
            DropColumn("pnp.Packages", "SortOrder");
            DropColumn("pnp.Packages", "Abstract");
        }
    }
}
