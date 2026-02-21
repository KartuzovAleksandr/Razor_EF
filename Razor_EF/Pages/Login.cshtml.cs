using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using Razor_EF.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
// псевдоним, т.к. имена пакета и класса совпадают
using BC = BCrypt.Net.BCrypt;

namespace Razor_EF.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LoginModel> _logger;
        private readonly IConfiguration _config;

        [BindProperty]
        public User User { get; set; } = new();

        public LoginModel(ApplicationDbContext context, 
                            ILogger<LoginModel> logger,
                            IConfiguration config)
        {
            _context = context;
            _logger = logger;
            _config = config;
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
                // user.Password - hash из БД, User.Password - ввел пользователь
                if (!BC.Verify(User.Password, user.Password))
                // простая проверка, когда пароль был в открытом виде 
                // if (user.Password != User.Password)
                {
                    ModelState.AddModelError("User.UserName", "Пароль не верен !");
                    return Page();
                }

                // --- ГЕНЕРАЦИЯ JWT ТОКЕНА ---
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

                // Добавляем утверждения (Claims)
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, user.UserName),
                    new(ClaimTypes.Role, user.Role.ToString()), // Важно для проверки ролей
                    new("UserId", user.Id.ToString())
                };
                _logger.LogInformation($"Claim с ролью {user.Role.ToString()} входит в систему !");

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddHours(1), // Время жизни токена
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                // Сохраняем токен в Cookie
                Response.Cookies.Append("AccessToken", tokenString, new CookieOptions
                {
                    HttpOnly = true, // Защита от XSS
                    Secure = true,  // Поставьте true, если используете HTTPS
                    Expires = DateTime.UtcNow.AddHours(1),
                    SameSite = SameSiteMode.Lax
                });

                _logger.LogInformation($"{user.ToString()} вошел в систему !");
                return RedirectToPage("/Index");
            }
        }
    }
}