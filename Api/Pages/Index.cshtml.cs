namespace Authentication.Pages
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc.RazorPages;

    [Authorize(Roles = "Admin")]
    [ExcludeFromCodeCoverage]
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
