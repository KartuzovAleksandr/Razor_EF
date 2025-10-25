using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Razor_EF.Pages
{
    public class TestErrorModel : PageModel
    {
        public void OnGet()
        {
            throw new DivideByZeroException("Тест деления на ноль!");
        }
    }
}
