namespace Authentication.Pages.PersistedGrants.Details
{
    using System.Threading.Tasks;
    using IdentityServer4.EntityFramework.Entities;
    using IdentityServer4.EntityFramework.Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;

    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IPersistedGrantDbContext _context;

        public IndexModel(IPersistedGrantDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public PersistedGrant PersistedGrant { get; set; }

        public async Task<IActionResult> OnGetAsync([FromRoute] string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return NotFound();
            }

            PersistedGrant = await _context.PersistedGrants.FindAsync(key).ConfigureAwait(false);
            if (PersistedGrant == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
