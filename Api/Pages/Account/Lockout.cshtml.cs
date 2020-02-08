namespace Authentication.Pages.Account
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;

    [AllowAnonymous]
    [ExcludeFromCodeCoverage]
    public class LockoutModel : PageModel
    {
        [TempData]
        [ViewData]
        public string Origin { get; set; }

        public void OnGet()
        {
        }
    }
}
