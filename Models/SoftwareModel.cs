using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InzV3.Models
{

    public class SoftwareModel
    {
        [Key]
        public int id_software { get; set; }
        [Required(ErrorMessage = "Nazwa oprogramowania jest wymagana.")]
        public string software_name { get; set; }

        [DisplayName("Numer seryjny oprogramowania")]
        public string software_serial_number { get; set; }
        [DisplayName("Klucz licencji")]
        public string license_key { get; set; }
        [DisplayName("Data końca licencji")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString="{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? license_end_date { get; set; }
        [Required]
        [DisplayName("Kategoria")]
        public string software_category { get; set; }
        [DisplayName("Przypisane urządzenie")]
        public int? assigned_device { get; set; }
        [ForeignKey("assigned_device")]
        public virtual DeviceModel id_device { get; set; }
    }
    
}
