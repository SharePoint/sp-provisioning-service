namespace SharePointPnP.ProvisioningApp.DomainModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Wave9OAuthConsumerApps20190730 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "pnp.ConsumerApps",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        DisplayName = c.String(),
                        ContactEmail = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("pnp.ConsumerApps");
        }
    }
}
