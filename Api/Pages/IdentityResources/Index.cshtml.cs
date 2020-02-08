namespace Authentication.Pages.IdentityResources
{
    using System.Collections.Generic;
    using IdentityServer4.EntityFramework.Entities;
    using IdentityServer4.EntityFramework.Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc.RazorPages;

    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IConfigurationDbContext _context;

        public IndexModel(IConfigurationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<IdentityResource> IdentityResources { get; set; }

        public void OnGet()
        {
            IdentityResources = _context.IdentityResources;
        }
    }
}