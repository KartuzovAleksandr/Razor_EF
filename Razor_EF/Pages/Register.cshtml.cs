using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Razor_EF.Models;
using BC = BCrypt.Net.BCrypt;

namespace Razor_EF.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RegisterModel> _logger;

        [BindProperty]
        public User User { get; set; } = new();

        public RegisterModel(ApplicationDbContext context, ILogger<RegisterModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public IActionResult OnPostCreate()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            
            if (!_context.Users.Any(u => u.UserName == User.UserName))
            {
                // шифрую (хэширую) пароль
                User.Password = BC.HashPassword(User.Password);
                _context.Users.Add(User);
                _context.SaveChanges();
                _logger.LogInformation($"{User.ToString()} зарегистрирован !");
                return RedirectToPage("/Login");
            }
            else
            {
                ModelState.AddModelError("User.UserName", "Пользователь с таким именем уже существует !");
                return Page();
            }
        }
    }
}