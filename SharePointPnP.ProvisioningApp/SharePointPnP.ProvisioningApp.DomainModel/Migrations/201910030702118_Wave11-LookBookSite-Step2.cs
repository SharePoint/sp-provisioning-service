namespace SharePointPnP.ProvisioningApp.DomainModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Wave11LookBookSiteStep2 : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("pnp.PageTemplates");
            AddColumn("pnp.PageTemplates", "Html", c => c.String(nullable: false));
            AddColumn("pnp.PageTemplates", "Css", c => c.String(nullable: false));
            AlterColumn("pnp.Packages", "PageTemplateId", c => c.String());
            AlterColumn("pnp.PageTemplates", "Id", c => c.String(nullable: false, maxLength: 128));
            AddPrimaryKey("pnp.PageTemplates", "Id");
            DropColumn("pnp.PageTemplates", "HtmlFile");
            DropColumn("pnp.PageTemplates", "CssFile");
        }
        
        public override void Down()
        {
            AddColumn("pnp.PageTemplates", "CssFile", c => c.String(nullable: false, maxLength: 500));
            AddColumn("pnp.PageTemplates", "HtmlFile", c => c.String(nullable: false, maxLength: 500));
            DropPrimaryKey("pnp.PageTemplates");
            AlterColumn("pnp.PageTemplates", "Id", c => c.Guid(nullable: false, identity: true));
            AlterColumn("pnp.Packages", "PageTemplateId", c => c.Guid(nullable: false));
            DropColumn("pnp.PageTemplates", "Css");
            DropColumn("pnp.PageTemplates", "Html");
            AddPrimaryKey("pnp.PageTemplates", "Id");
        }
    }
}
