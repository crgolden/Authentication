namespace Authentication.Pages.Clients
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

        public IEnumerable<Client> Clients { get; set; }

        public void OnGet()
        {
            Clients = _context.Clients;
        }
    }
}