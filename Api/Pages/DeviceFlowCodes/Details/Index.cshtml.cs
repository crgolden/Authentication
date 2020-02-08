namespace Authentication.Pages.DeviceFlowCodes.Details
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
        public DeviceFlowCodes DeviceFlowCode { get; set; }

        public async Task<IActionResult> OnGetAsync([FromRoute] string userCode)
        {
            if (string.IsNullOrEmpty(userCode))
            {
                return NotFound();
            }

            DeviceFlowCode = await _context.DeviceFlowCodes.FindAsync(userCode).ConfigureAwait(false);
            if (DeviceFlowCode == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
