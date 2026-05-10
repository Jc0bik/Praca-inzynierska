using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNet.Identity.EntityFramework;

namespace InzV3.Models
{
    public class SubRoleModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [DisplayName("Nazwa podroli")]
        public string Name { get; set; }
        public string RoleId { get; set; }
        [ForeignKey("RoleId")]
        public virtual IdentityRole Role { get; set; }
        [Required]
        [DisplayName("Interwał wymuszenia ocen (liczony w dniach)")]
        public int RatingIntervalDays { get; set; } = 30;
    }
}