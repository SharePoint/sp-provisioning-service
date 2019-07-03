namespace SharePointPnP.ProvisioningApp.DomainModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Wave6ProvisioningActionItems320190703 : DbMigration
    {
        public override void Up()
        {
            AddColumn("pnp.ProvisioningActionItems", "FailedOn", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("pnp.ProvisioningActionItems", "FailedOn");
        }
    }
}
