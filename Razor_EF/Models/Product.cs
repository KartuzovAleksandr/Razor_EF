using System.ComponentModel.DataAnnotations;

namespace Razor_EF.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Название обязательно")]
        [Display(Name = "Название")]
        [StringLength(35, MinimumLength = 2, ErrorMessage = "Название должно быть от 2 до 35 символов")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Цена обязательна")]
        [Range(1, 5000000, ErrorMessage = "Цена должна быть от 1 до 5 000 000")]
        [Display(Name = "Цена")]
        public decimal Price { get; set; }
    }
}