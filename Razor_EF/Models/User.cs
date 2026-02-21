using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Razor_EF.Models
{
    public enum Roles
    {
        User,
        Manager,
        Admin
    }
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Логин")]
        [Required(ErrorMessage = "Имя обязательно")]
        [StringLength(10, MinimumLength = 5, ErrorMessage = "Имя должно быть от 5 до 10 символов")]
        public String UserName { get; set; } = String.Empty;

        [Display(Name = "Пароль")]
        [Required(ErrorMessage = "Пароль обязателен")]
        [StringLength(60, MinimumLength = 1, ErrorMessage = "Пароль должен быть от 1 до 10 символов")]
        // автоматически рендерится как <input type="password"> c tag-helpers
        [DataType(DataType.Password)]
        // Для базы данных выделяем 60 символов под хэш
        // не проходит в Postgres - поэтому берется из правила валидации выше
        // [Column(TypeName = "text")]
        public String Password { get; set; } = String.Empty;

        [Display(Name = "Роль")]
        public Roles Role { get; set; }

        public override String ToString()
        {
            return $"Имя: {UserName} Роль: {Role}";
        }
    }
}