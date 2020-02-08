namespace Authentication.Pages.PersistedGrants
{
    using System.Collections.Generic;
    using IdentityServer4.EntityFramework.Entities;
    using IdentityServer4.EntityFramework.Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc.RazorPages;

    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IPersistedGrantDbContext _context;

        public IndexModel(IPersistedGrantDbContext context)
        {
            _context = context;
        }

        public IEnumerable<PersistedGrant> PersistedGrants { get; set; }

        public void OnGet()
        {
            PersistedGrants = _context.PersistedGrants;
        }
    }
}