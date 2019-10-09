namespace SharePointPnP.ProvisioningApp.DomainModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Wave11LookBookSiteStep5 : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "pnp.PackagesPlatforms", name: "CategoryId", newName: "PlatformId");
            RenameIndex(table: "pnp.PackagesPlatforms", name: "IX_CategoryId", newName: "IX_PlatformId");
        }
        
        public override void Down()
        {
            RenameIndex(table: "pnp.PackagesPlatforms", name: "IX_PlatformId", newName: "IX_CategoryId");
            RenameColumn(table: "pnp.PackagesPlatforms", name: "PlatformId", newName: "CategoryId");
        }
    }
}
