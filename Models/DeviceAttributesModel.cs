using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace InzV3.Models
{
    public class DevCategory
    {
        [Key]
        [DisplayName("ID kategorii")]
        public int id_category { get; set; }

        [Required]
        [DisplayName("Nazwa kategorii")]
        public string category_name { get; set; }
        public DevCategory(int id_category, string category_name)
        {
            this.id_category = id_category;
            this.category_name = category_name;
        }
        public DevCategory() { }
    }
    public class DevFeature
    {
        [Key]
        [DisplayName("ID cechy szczególnej")]
        public int id_dev_feature { get; set; }
        [Required]
        [DisplayName("Nazwa cechy szczególnej")]
        public string dev_feature_name { get; set; }
        public DevFeature()
        {
            id_dev_feature = -1;
            dev_feature_name = "N/a";
        }
        public DevFeature(int id_dev_feature, string dev_feature_name)
        {
            this.id_dev_feature = id_dev_feature;
            this.dev_feature_name = dev_feature_name;
        }
    }
    public class DevCharacteristic
    {
        [Key]
        public int id_dev_characteristic { get; set; }

        [DisplayName("ID urządzenia")]
        public int id_device { get; set; }
        public int id_dev_feature { get; set; }
        public int id_category { get; set; }
        [DisplayName("Wartość cechy szczególnej")]
        public string dev_feature_value { get; set; }
        //Przypisanie foreign key
        [ForeignKey("id_device")]
        public virtual DeviceModel DeviceModel { get; set; }
        [ForeignKey("id_dev_feature")]
        public virtual DevFeature DevFeature { get; set; }
        [ForeignKey("id_category")]
        public virtual DevCategory DevCategory { get; set; }
        public DevCharacteristic() { }

    }
}