namespace Authentication.Pages.DeviceFlowCodes
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

        public IEnumerable<DeviceFlowCodes> DeviceFlowCodes { get; set; }

        public void OnGet()
        {
            DeviceFlowCodes = _context.DeviceFlowCodes;
        }
    }
}