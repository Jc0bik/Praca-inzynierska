namespace InzV3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PoprawionaBazaDevices : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DevCategories",
                c => new
                    {
                        id_category = c.Int(nullable: false, identity: true),
                        category_name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.id_category);
            
            CreateTable(
                "dbo.DevCharacteristics",
                c => new
                    {
                        id_dev_characteristic = c.Int(nullable: false, identity: true),
                        id_device = c.Int(nullable: false),
                        id_dev_feature = c.Int(nullable: false),
                        id_category = c.Int(nullable: false),
                        dev_feature_value = c.String(),
                    })
                .PrimaryKey(t => t.id_dev_characteristic)
                .ForeignKey("dbo.DevCategories", t => t.id_category, cascadeDelete: true)
                .ForeignKey("dbo.DevFeatures", t => t.id_dev_feature, cascadeDelete: true)
                .ForeignKey("dbo.DeviceModels", t => t.id_device, cascadeDelete: true)
                .Index(t => t.id_device)
                .Index(t => t.id_dev_feature)
                .Index(t => t.id_category);
            
            CreateTable(
                "dbo.DevFeatures",
                c => new
                    {
                        id_dev_feature = c.Int(nullable: false, identity: true),
                        dev_feature_name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.id_dev_feature);
            
            CreateTable(
                "dbo.DeviceModels",
                c => new
                    {
                        id_device = c.Int(nullable: false, identity: true),
                        brand = c.String(nullable: false),
                        model = c.String(nullable: false),
                        serial_num = c.String(nullable: false),
                        status = c.String(nullable: false),
                        warranty = c.DateTime(),
                        id_user = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id_device);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.DevCharacteristics", "id_device", "dbo.DeviceModels");
            DropForeignKey("dbo.DevCharacteristics", "id_dev_feature", "dbo.DevFeatures");
            DropForeignKey("dbo.DevCharacteristics", "id_category", "dbo.DevCategories");
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.DevCharacteristics", new[] { "id_category" });
            DropIndex("dbo.DevCharacteristics", new[] { "id_dev_feature" });
            DropIndex("dbo.DevCharacteristics", new[] { "id_device" });
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.DeviceModels");
            DropTable("dbo.DevFeatures");
            DropTable("dbo.DevCharacteristics");
            DropTable("dbo.DevCategories");
        }
    }
}
