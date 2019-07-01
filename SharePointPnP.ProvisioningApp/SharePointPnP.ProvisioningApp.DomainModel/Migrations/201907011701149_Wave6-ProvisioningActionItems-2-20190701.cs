namespace SharePointPnP.ProvisioningApp.DomainModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Wave6ProvisioningActionItems220190701 : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("pnp.ProvisioningActionItems");
            AlterColumn("pnp.ProvisioningActionItems", "Id", c => c.Guid(nullable: false));
            AddPrimaryKey("pnp.ProvisioningActionItems", "Id");
        }
        
        public override void Down()
        {
            DropPrimaryKey("pnp.ProvisioningActionItems");
            AlterColumn("pnp.ProvisioningActionItems", "Id", c => c.Guid(nullable: false, identity: true));
            AddPrimaryKey("pnp.ProvisioningActionItems", "Id");
        }
    }
}
