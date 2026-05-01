namespace InzV3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DodanieWalidacjiDoUrządzeń : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.DeviceModels", "warranty", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.DeviceModels", "warranty", c => c.DateTime());
        }
    }
}
