using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Razor_EF.Models;

namespace Razor_EF.Pages
{
    public class OrderModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public OrderModel(ApplicationDbContext context) => _context = context;

        public IList<Order> Orders { get; set; } = new List<Order>();
        public IList<Client> Clients { get; set; } = new List<Client>();
        public IList<Product> Products { get; set; } = new List<Product>();

        [BindProperty] public Order Order { get; set; } = new();

        public void OnGet()
        {
            Orders = _context.Orders
                .Include(o => o.Client)
                .Include(o => o.Product)
                .ToList();
            Clients = _context.Clients.ToList();
            Products = _context.Products.ToList();
        }

        public IActionResult OnPostCreate()
        {
            if (ModelState.IsValid)
            {
                // для PostgreSQL
                Order.Date = DateTime.Now.ToUniversalTime();
                _context.Orders.Add(Order);
                _context.SaveChanges();
                return RedirectToPage();
            }
            LoadLists();
            return Page();
        }

        public IActionResult OnPostEdit(int id)
        {
            var o = _context.Orders.Find(id);
            if (o != null)
            {
                o.ClientId = Order.ClientId;
                o.ProductId = Order.ProductId;
                o.Quantity = Order.Quantity;
                _context.SaveChanges();
            }
            return RedirectToPage();
        }

        public IActionResult OnPostDelete(int id)
        {
            var o = _context.Orders.Find(id);
            if (o != null)
            {
                _context.Orders.Remove(o);
                _context.SaveChanges();
            }
            return RedirectToPage();
        }

        private void LoadLists()
        {
            Clients = _context.Clients.ToList();
            Products = _context.Products.ToList();
        }
    }
}