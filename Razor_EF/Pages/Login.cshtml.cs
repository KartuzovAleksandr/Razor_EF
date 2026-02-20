using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Razor_EF.Models;
using BC = BCrypt.Net.BCrypt;

namespace Razor_EF.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LoginModel> _logger;

        [BindProperty]
        public User User { get; set; } = new();

        public LoginModel(ApplicationDbContext context, ILogger<LoginModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public IActionResult OnPostLogin()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // ищем пользователя с этим именем
            var user = _context.Users.FirstOrDefault(u => u.UserName == User.UserName);

            if (user is null)
            {
                ModelState.AddModelError("User.UserName", "Пользователь не найден !");
                return Page();
            }
            else
            {
                // проверка введенного пароля
                // user.Password - hash, User.Password - ввел пользователь
                if (!BC.Verify(User.Password, user.Password))
                // if (user.Password != User.Password)
                {
                    ModelState.AddModelError("User.UserName", "Пароль не верен !");
                    return Page();
                }
                _logger.LogInformation($"{User.ToString()} вошел в систему !");
                return RedirectToPage("/Index");
            }
        }
    }
}