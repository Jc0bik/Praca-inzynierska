using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Web.ModelBinding;
namespace InzV3.Models
{
    public class DeviceModel 
    {
        [Key]
        [DisplayName("ID urządzenia")]
        public int id_device {  get; set; }
        [Required]
        [DisplayName("Marka")]
        public string brand { get; set; }
        [Required]
        [DisplayName("Model")]
        public string model { get; set; }
        [Required]
        [DisplayName("Numer Seryjny")]
        public string serial_num { get; set; }
        [Required]
        [DisplayName("Status urządzenia")]
        public string status { get; set; }
        [Required]
        [DisplayName("Data końcowa gwarancji")]
        public DateTime? warranty { get; set; }
        [DisplayName("Przypisany użytkownik")]
        public string id_user { get; set; }
        [ForeignKey("id_user")]
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<DevCharacteristic> Charachteristics { get; set; }

        public DeviceModel()
        {
            id_device = -1;
            brand = "";
            model = "";
            serial_num = "";
            status = "N/a";
            warranty = null;
            id_user = null;
            Charachteristics = new HashSet<DevCharacteristic>();
        }

        public DeviceModel(int id_device, string brand, string model, string serial_num, string status, DateTime? warranty, string id_user)
        {
            this.id_device = id_device;
            this.brand = brand;
            this.model = model;
            this.serial_num = serial_num;
            this.status = status;
            this.warranty = warranty;
            this.id_user = id_user;
            Charachteristics = new HashSet<DevCharacteristic>();
        }
    }
}