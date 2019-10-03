namespace SharePointPnP.ProvisioningApp.DomainModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Wave10LookBookSite : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "pnp.PageTemplates",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        HtmlFile = c.String(nullable: false, maxLength: 500),
                        CssFile = c.String(nullable: false, maxLength: 500),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("pnp.Packages", "PageTemplateId", c => c.Guid(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("pnp.Packages", "PageTemplateId");
            DropTable("pnp.PageTemplates");
        }
    }
}
