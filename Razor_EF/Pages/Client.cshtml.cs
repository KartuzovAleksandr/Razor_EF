using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Razor_EF.Models;

namespace Razor_EF.Pages
{
    public class ClientModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ClientModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Client> Clients { get; set; } = new List<Client>();

        [BindProperty]
        public Client Client { get; set; } = new();

        public void OnGet()
        {
            Clients = _context.Clients.ToList();
        }

        public IActionResult OnPostCreate()
        {
            if (ModelState.IsValid)
            {
                _context.Clients.Add(Client);
                _context.SaveChanges();
                return RedirectToPage();
            }
            Clients = _context.Clients.ToList();
            return Page();
        }

        public IActionResult OnPostEdit(int id)
        {
            var existing = _context.Clients.Find(id);
            if (existing != null)
            {
                existing.Email = Client.Email;
                _context.SaveChanges();
            }
            return RedirectToPage();
        }

        public IActionResult OnPostDelete(int id)
        {
            var client = _context.Clients.Find(id);
            if (client != null)
            {
                _context.Clients.Remove(client);
                _context.SaveChanges();
            }
            return RedirectToPage();
        }
    }
}