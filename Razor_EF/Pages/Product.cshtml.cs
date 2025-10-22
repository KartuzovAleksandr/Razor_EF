// Аналогично ClientModel, но с Product
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Razor_EF.Models;

namespace Razor_EF.Pages
{
    public class ProductModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ProductModel(ApplicationDbContext context) => _context = context;

        public IList<Product> Products { get; set; } = new List<Product>();
        [BindProperty] public Product Product { get; set; } = new();

        public void OnGet() => Products = _context.Products.ToList();

        public IActionResult OnPostCreate()
        {
            if (ModelState.IsValid)
            {
                _context.Products.Add(Product);
                _context.SaveChanges();
                return RedirectToPage();
            }
            Products = _context.Products.ToList();
            return Page();
        }

        public IActionResult OnPostEdit(int id)
        {
            var p = _context.Products.Find(id);
            if (p != null)
            {
                p.Name = Product.Name;
                p.Price = Product.Price;
                _context.SaveChanges();
            }
            return RedirectToPage();
        }

        public IActionResult OnPostDelete(int id)
        {
            var p = _context.Products.Find(id);
            if (p != null)
            {
                _context.Products.Remove(p);
                _context.SaveChanges();
            }
            return RedirectToPage();
        }
    }
}