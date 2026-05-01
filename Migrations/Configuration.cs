namespace InzV3.Migrations
{
    using InzV3.Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<InzV3.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(InzV3.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
             if (!roleManager.RoleExists("Admin"))
            {
                roleManager.Create(new IdentityRole("Admin"));
            }
            if (userManager.FindByEmail("s217526@sggw.edu.pl")==null)
            {
                var user = new ApplicationUser { UserName = "admin", Email = "s217526@sggw.edu.pl" };
                userManager.Create(user, "Mazda5.123");
                userManager.AddToRole(user.Id, "Admin");
            }
            if(!roleManager.RoleExists("Użytkownik"))
            {
                roleManager.Create(new IdentityRole("Użytkownik"));
            }
            if(!roleManager.RoleExists("Pracownik Serwisu")) 
            {
                roleManager.Create(new IdentityRole("Pracownik Serwisu")); 
            }
        }
    }
}
