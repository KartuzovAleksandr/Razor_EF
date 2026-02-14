using Microsoft.VisualStudio.TextTemplating;
using System.ComponentModel.DataAnnotations;

namespace Razor_EF.Models
{
    public class Client
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Имя обязательно")]
        [Display(Name = "Имя")]
        [StringLength(50, ErrorMessage = "Имя не должно быть длиннее 50 символов")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Неверный формат email")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        public override String ToString()
        {
            return $"Имя: {Name} Email: {Email}";
        }
    }
}