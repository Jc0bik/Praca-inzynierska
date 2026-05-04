namespace InzV3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DodanieZgloszen : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TicketMessageModels",
                c => new
                    {
                        id_message = c.Int(nullable: false, identity: true),
                        messageContent = c.String(nullable: false),
                        createdAt = c.DateTime(nullable: false),
                        id_ticket = c.Int(nullable: false),
                        id_sender = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.id_message)
                .ForeignKey("dbo.AspNetUsers", t => t.id_sender)
                .ForeignKey("dbo.TicketModels", t => t.id_ticket, cascadeDelete: true)
                .Index(t => t.id_ticket)
                .Index(t => t.id_sender);
            
            CreateTable(
                "dbo.TicketModels",
                c => new
                    {
                        id_ticket = c.Int(nullable: false, identity: true),
                        description = c.String(nullable: false),
                        status = c.String(),
                        createdAt = c.DateTime(nullable: false),
                        updatedAt = c.DateTime(nullable: false),
                        id_user = c.String(nullable: false, maxLength: 128),
                        id_device = c.Int(nullable: false),
                        id_technician = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.id_ticket)
                .ForeignKey("dbo.DeviceModels", t => t.id_device, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.id_technician)
                .ForeignKey("dbo.AspNetUsers", t => t.id_user)
                .Index(t => t.id_user)
                .Index(t => t.id_device)
                .Index(t => t.id_technician);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TicketModels", "id_user", "dbo.AspNetUsers");
            DropForeignKey("dbo.TicketModels", "id_technician", "dbo.AspNetUsers");
            DropForeignKey("dbo.TicketMessageModels", "id_ticket", "dbo.TicketModels");
            DropForeignKey("dbo.TicketModels", "id_device", "dbo.DeviceModels");
            DropForeignKey("dbo.TicketMessageModels", "id_sender", "dbo.AspNetUsers");
            DropIndex("dbo.TicketModels", new[] { "id_technician" });
            DropIndex("dbo.TicketModels", new[] { "id_device" });
            DropIndex("dbo.TicketModels", new[] { "id_user" });
            DropIndex("dbo.TicketMessageModels", new[] { "id_sender" });
            DropIndex("dbo.TicketMessageModels", new[] { "id_ticket" });
            DropTable("dbo.TicketModels");
            DropTable("dbo.TicketMessageModels");
        }
    }
}
