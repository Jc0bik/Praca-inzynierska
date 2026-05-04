using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace InzV3.Models
{
    public static class TicketStatuses
    {
       public const string Wyslane= "Wysłane";
       public const string Przyjete = "Przyjęte";
       public const string OczekujeUser = "Oczekuje na odpowiedź użytkownika";
       public const string OczekujeSerwis= "Oczekuje na odpowiedź serwisu";
       public const string IdzDoSerwisu = "Proszę udać się do serwisu";
       public const string WRealizacji = "W realizacji";
       public const string DoOdbioru = "Oczekuje na odbiór";
       public const string Zamkniete = "Zamknięte";
    }

    public class TicketModel
    {
        [Key]
        [Display(Name ="ID zgłoszenia")]
        public int id_ticket { get; set; }
        [Required]
        [Display(Name = "Opis problemu")]
        public string description { get; set; }
        [Display(Name ="Status")]
        public string status { get; set; }
        [Display(Name = "Data zgłoszenia")]
        public DateTime createdAt {  get; set; }
        [Display(Name = "Ostatniej aktualizacja")]
        public DateTime updatedAt { get; set; }

        [Required]
        public string id_user { get; set; }
        [ForeignKey("id_user")]
        public virtual ApplicationUser User { get; set; }

        [Required]
        public int id_device { get; set; }
        [ForeignKey("id_device")]
        public virtual DeviceModel Device { get; set; }
        public string id_technician { get; set; }
        [ForeignKey("id_technician")]
        public virtual ApplicationUser Technician { get; set; }

        public virtual ICollection<TicketMessageModel> Messages { get; set; }
        public TicketModel()
        {
            Messages= new HashSet<TicketMessageModel>();
        }
    }
}