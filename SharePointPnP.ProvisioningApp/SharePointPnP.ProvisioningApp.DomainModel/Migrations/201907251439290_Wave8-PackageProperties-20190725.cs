namespace SharePointPnP.ProvisioningApp.DomainModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Wave8PackageProperties20190725 : DbMigration
    {
        public override void Up()
        {
            AddColumn("pnp.Packages", "PackageProperties", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("pnp.Packages", "PackageProperties");
        }
    }
}
