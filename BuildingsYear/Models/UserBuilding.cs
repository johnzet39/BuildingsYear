using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BuildingsYear.Models
{
    public class UserBuilding
    {
        [Required]
        [Display(Name = "Уникальный идентификатор")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "UPRN must be numeric")]
        
        public int Keyid { get; set; }

        [Required(ErrorMessage = "Не указан год постройки")]
        [Display(Name = "Год постройки (или середина периода)")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "UPRN must be numeric")]
        public int? YearNew { get; set; } = null;

        [Required(ErrorMessage = "Не указан источник информации")]
        [Display(Name = "Источник информации")]
        [MaxLength(50)]
        public string ReasonYear { get; set; }

        [Display(Name = "Ваше имя")]
        [MaxLength(50)]
        public string Username { get; set; }

        [Display(Name = "Email для возможности обратной связи")]
        [EmailAddress(ErrorMessage = "Некорректный Email-адрес")]
        public string UserMail { get; set; }

        public DateTime DateInsert { get; set; }
        public string IpAddressUser { get; set; }
    }
}
