using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace InzV3.Models
{
    // Możesz dodać dane profilu dla użytkownika, dodając więcej właściwości do klasy ApplicationUser. Odwiedź stronę https://go.microsoft.com/fwlink/?LinkID=317594, aby dowiedzieć się więcej.
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? SubRoleId { get; set; }
        [ForeignKey("SubRoleId")]
        public virtual SubRoleModel SubRole { get; set; }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            
            // Element authenticationType musi pasować do elementu zdefiniowanego w elemencie CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Dodaj tutaj niestandardowe oświadczenia użytkownika
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<DeviceModel> Devices { get; set; }
        public DbSet<SoftwareModel> Software { get; set; }
        public DbSet<DevCategory> DevCategories { get; set; }
        public DbSet<DevCharacteristic> DevCharacteristics { get; set; }
        public DbSet<DevFeature> DevFeatures { get; set; }
        public DbSet<DeviceRating> DeviceRating { get; set; }
        public DbSet<SubRoleModel> SubRoles { get; set; }
        public DbSet<TicketModel> Tickets { get; set; }
        public DbSet<TicketMessageModel> TicketMessages { get; set; }

        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity <TicketModel>()
                .HasRequired(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.id_user)
                .WillCascadeOnDelete(false);
            modelBuilder .Entity<TicketModel>()
                .HasOptional(t => t.Technician)
                .WithMany()
                .HasForeignKey(t => t.id_technician)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<TicketMessageModel>()
                .HasRequired(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.id_sender)
                .WillCascadeOnDelete(false);
        }
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}