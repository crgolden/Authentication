namespace Authentication.Pages.PersistedGrants
{
    using System.Threading.Tasks;
    using IdentityServer4.EntityFramework.Entities;
    using IdentityServer4.EntityFramework.Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;

    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly IPersistedGrantDbContext _context;

        public DeleteModel(IPersistedGrantDbContext context)
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

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(PersistedGrant.Key))
            {
                return Page();
            }

            var persistedGrant = await _context.PersistedGrants.FindAsync(PersistedGrant.Key).ConfigureAwait(false);
            if (persistedGrant == null)
            {
                return Page();
            }

            _context.PersistedGrants.Remove(persistedGrant);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return RedirectToPage("./Index");
        }
    }
}
