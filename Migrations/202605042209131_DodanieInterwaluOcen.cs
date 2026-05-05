namespace InzV3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DodanieInterwaluOcen : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DeviceModels", "last_rated_at", c => c.DateTime());
            AddColumn("dbo.SubRoleModels", "RatingIntervalDays", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SubRoleModels", "RatingIntervalDays");
            DropColumn("dbo.DeviceModels", "last_rated_at");
        }
    }
}
