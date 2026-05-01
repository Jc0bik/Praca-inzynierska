namespace InzV3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dodanieOprogramowania : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SoftwareModels",
                c => new
                    {
                        id_software = c.Int(nullable: false, identity: true),
                        software_name = c.String(nullable: false),
                        software_serial_number = c.String(),
                        license_key = c.String(),
                        license_end_date = c.DateTime(),
                        software_category = c.String(nullable: false),
                        assigned_device = c.Int(),
                    })
                .PrimaryKey(t => t.id_software)
                .ForeignKey("dbo.DeviceModels", t => t.assigned_device)
                .Index(t => t.assigned_device);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SoftwareModels", "assigned_device", "dbo.DeviceModels");
            DropIndex("dbo.SoftwareModels", new[] { "assigned_device" });
            DropTable("dbo.SoftwareModels");
        }
    }
}
