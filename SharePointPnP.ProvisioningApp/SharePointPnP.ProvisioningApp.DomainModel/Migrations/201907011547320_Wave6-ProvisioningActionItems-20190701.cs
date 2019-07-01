namespace SharePointPnP.ProvisioningApp.DomainModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Wave6ProvisioningActionItems20190701 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "pnp.ProvisioningActionItems",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        TenantId = c.Guid(nullable: false),
                        PackageId = c.Guid(nullable: false),
                        PackageProperties = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        ExpiresOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => new { t.TenantId, t.PackageId });
            
        }
        
        public override void Down()
        {
            DropIndex("pnp.ProvisioningActionItems", new[] { "TenantId", "PackageId" });
            DropTable("pnp.ProvisioningActionItems");
        }
    }
}
