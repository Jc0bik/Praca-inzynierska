using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InzV3.Models
{

    public class DeviceRating
    {
        [Key]
        public int id_rating { get; set; }
        [Required]
        public int id_device { get; set; }
        [Required]
        [StringLength(100)]
        [Display(Name ="Kategoria oceny")]
        public string category_name { get; set; }
        [Range(1, 5)]
        [Display(Name = "Ocena")]
        public int rating_value { get; set; } = 5;
        [ForeignKey("id_device")]
        public virtual DeviceModel DeviceModel { get; set; }
    }
    
}
