namespace InzV3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RelacjaPodroli : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "SubRoleId", c => c.Int());
            AddColumn("dbo.SubRoleModels", "RoleId", c => c.String(maxLength: 128));
            CreateIndex("dbo.AspNetUsers", "SubRoleId");
            CreateIndex("dbo.SubRoleModels", "RoleId");
            AddForeignKey("dbo.SubRoleModels", "RoleId", "dbo.AspNetRoles", "Id");
            AddForeignKey("dbo.AspNetUsers", "SubRoleId", "dbo.SubRoleModels", "Id");
            DropColumn("dbo.AspNetUsers", "SubRole");
            DropColumn("dbo.SubRoleModels", "ParentRoleName");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SubRoleModels", "ParentRoleName", c => c.String(nullable: false));
            AddColumn("dbo.AspNetUsers", "SubRole", c => c.String());
            DropForeignKey("dbo.AspNetUsers", "SubRoleId", "dbo.SubRoleModels");
            DropForeignKey("dbo.SubRoleModels", "RoleId", "dbo.AspNetRoles");
            DropIndex("dbo.SubRoleModels", new[] { "RoleId" });
            DropIndex("dbo.AspNetUsers", new[] { "SubRoleId" });
            DropColumn("dbo.SubRoleModels", "RoleId");
            DropColumn("dbo.AspNetUsers", "SubRoleId");
        }
    }
}
