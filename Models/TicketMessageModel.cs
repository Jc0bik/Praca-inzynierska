using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace InzV3.Models
{
    public class TicketMessageModel
    {
        [Key]
        public int id_message { get; set; }
        [Required]
        public string messageContent { get; set; }

        public DateTime createdAt { get; set; }

        [Required]
        public int id_ticket { get; set; }
        [ForeignKey("id_ticket")]
        public virtual TicketModel Ticket { get; set; }

        [Required]
        public string id_sender { get; set; }
        [ForeignKey("id_sender")]
        public virtual ApplicationUser Sender { get; set; }
    }
}