namespace SharePointPnP.ProvisioningApp.DomainModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Wave11LookBookSiteStep4 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "pnp.Platforms",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        DisplayName = c.String(nullable: false, maxLength: 200),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "pnp.PackagesPlatforms",
                c => new
                    {
                        PackageId = c.Guid(nullable: false),
                        CategoryId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.PackageId, t.CategoryId })
                .ForeignKey("pnp.Packages", t => t.PackageId, cascadeDelete: true)
                .ForeignKey("pnp.Platforms", t => t.CategoryId, cascadeDelete: true)
                .Index(t => t.PackageId)
                .Index(t => t.CategoryId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("pnp.PackagesPlatforms", "CategoryId", "pnp.Platforms");
            DropForeignKey("pnp.PackagesPlatforms", "PackageId", "pnp.Packages");
            DropIndex("pnp.PackagesPlatforms", new[] { "CategoryId" });
            DropIndex("pnp.PackagesPlatforms", new[] { "PackageId" });
            DropTable("pnp.PackagesPlatforms");
            DropTable("pnp.Platforms");
        }
    }
}
