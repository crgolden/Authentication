namespace Authentication.Pages.ApiResources
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
        public ApiResource ApiResource { get; set; }

        public IActionResult OnGet()
        {
            ApiResource = new ApiResource();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ApiResource.Created = DateTime.UtcNow;
            _context.ApiResources.Add(ApiResource);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return RedirectToPage("./Details/Index", new { ApiResource.Id });
        }
    }
}