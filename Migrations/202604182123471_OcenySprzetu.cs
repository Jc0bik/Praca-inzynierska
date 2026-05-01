namespace InzV3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OcenySprzetu : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DeviceRatings",
                c => new
                    {
                        id_rating = c.Int(nullable: false, identity: true),
                        id_device = c.Int(nullable: false),
                        category_name = c.String(nullable: false, maxLength: 100),
                        rating_value = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id_rating)
                .ForeignKey("dbo.DeviceModels", t => t.id_device, cascadeDelete: true)
                .Index(t => t.id_device);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DeviceRatings", "id_device", "dbo.DeviceModels");
            DropIndex("dbo.DeviceRatings", new[] { "id_device" });
            DropTable("dbo.DeviceRatings");
        }
    }
}
