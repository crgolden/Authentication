namespace Authentication.Pages.DeviceFlowCodes
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

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(DeviceFlowCode.UserCode))
            {
                return Page();
            }

            var deviceFlowCode = await _context.DeviceFlowCodes.FindAsync(DeviceFlowCode.UserCode).ConfigureAwait(false);
            if (deviceFlowCode == null)
            {
                return Page();
            }

            _context.DeviceFlowCodes.Remove(deviceFlowCode);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return RedirectToPage("./Index");
        }
    }
}
