using Microsoft.AspNetCore.Mvc.RazorPages;
using Razor_EF.Models;

namespace Razor_EF.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<IndexModel> _logger;

        // ≈динственный конструктор Ч внедр€ем и контекст, и логгер
        public IndexModel(ApplicationDbContext context, ILogger<IndexModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void OnGet()
        {
            _context.Database.EnsureCreated();
             if (!_context.Clients.Any())
             {
                 SeedData.Initialize(_context);
             }
        }
    }
}