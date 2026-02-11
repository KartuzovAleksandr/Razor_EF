using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Razor_EF.Models;

namespace Razor_EF.Pages.Products
{
    public class IndexModel : PageModel
    {
        private readonly Razor_EF.Models.ApplicationDbContext _context;

        public IndexModel(Razor_EF.Models.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Product> Product { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Product = await _context.Products.ToListAsync();
        }
    }
}
