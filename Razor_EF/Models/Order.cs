using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Razor_EF.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Display(Name = "Дата")]
        // для PostgreSQL
        //[Column(TypeName = "timestamp with time zone")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Клиент обязателен")]
        [Display(Name = "Клиент")]
        public int ClientId { get; set; }
        public Client? Client { get; set; }

        [Required(ErrorMessage = "Товар обязателен")]
        [Display(Name = "Товар")]
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Required(ErrorMessage = "Количество обязательно")]
        [Range(1, 1000, ErrorMessage = "Количество от 1 до 1000")]
        [Display(Name = "Количество")]
        public int Quantity { get; set; }
    }
}