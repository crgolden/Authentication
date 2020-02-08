namespace Authentication.Pages.Account
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc.RazorPages;

    [AllowAnonymous]
    [ExcludeFromCodeCoverage]
    public class RedirectModel : PageModel
    {
        public string RedirectUrl { get; set; }

        public void OnGet(string returnUrl = null)
        {
            RedirectUrl = returnUrl ?? Url.Content("~/");
        }
    }
}