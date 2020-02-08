namespace Authentication.Pages.IdentityResources
{
    using System;
    using System.Threading.Tasks;
    using IdentityServer4.EntityFramework.Entities;
    using IdentityServer4.EntityFramework.Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;

    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly IConfigurationDbContext _context;

        public CreateModel(IConfigurationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public IdentityResource IdentityResource { get; set; }

        public IActionResult OnGet()
        {
            IdentityResource = new IdentityResource();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            IdentityResource.Created = DateTime.UtcNow;
            _context.IdentityResources.Add(IdentityResource);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return RedirectToPage("./Details/Index", new { IdentityResource.Id });
        }
    }
}