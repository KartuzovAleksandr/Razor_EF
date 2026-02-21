// Аналогично ClientModel, но с Product
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Razor_EF.Models;
using System.Security.Claims;

namespace Razor_EF.Pages
{
    [Authorize(Roles = "Admin")]
    public class ProductModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductModel> _logger;

        public ProductModel(ApplicationDbContext context, ILogger<ProductModel> logger)
        {
            _context = context;
            _logger = logger;
        } 

        public IList<Product> Products { get; set; } = new List<Product>();
        [BindProperty] public Product Product { get; set; } = new();

        public void OnGet()
        {
            // _context.Products.ToList()
            Products = [.. _context.Products];
            // покажем пользователя и роль
            _logger.LogInformation($"Пользователь: {User.Identity.Name}, " +
                $"Роль: {string.Join(",", User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value))}" +
                $" вошел на страницу /Product");
        } 

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