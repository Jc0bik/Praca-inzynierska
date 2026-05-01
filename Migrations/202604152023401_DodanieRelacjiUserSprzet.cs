namespace InzV3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DodanieRelacjiUserSprzet : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.DeviceModels", "id_user", c => c.String(maxLength: 128));
            CreateIndex("dbo.DeviceModels", "id_user");
            AddForeignKey("dbo.DeviceModels", "id_user", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DeviceModels", "id_user", "dbo.AspNetUsers");
            DropIndex("dbo.DeviceModels", new[] { "id_user" });
            AlterColumn("dbo.DeviceModels", "id_user", c => c.Int(nullable: false));
        }
    }
}
