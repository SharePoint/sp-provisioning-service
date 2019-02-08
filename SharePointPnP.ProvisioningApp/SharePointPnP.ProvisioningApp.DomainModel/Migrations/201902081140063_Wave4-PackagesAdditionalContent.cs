namespace SharePointPnP.ProvisioningApp.DomainModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Wave4PackagesAdditionalContent : DbMigration
    {
        public override void Up()
        {
            AddColumn("pnp.Packages", "Instructions", c => c.String());
            AddColumn("pnp.Packages", "ProvisionRecap", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("pnp.Packages", "ProvisionRecap");
            DropColumn("pnp.Packages", "Instructions");
        }
    }
}
