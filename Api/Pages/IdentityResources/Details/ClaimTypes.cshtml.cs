namespace Authentication.Pages.IdentityResources.Details
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using IdentityServer4.EntityFramework.Entities;
    using IdentityServer4.EntityFramework.Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.EntityFrameworkCore;

    [Authorize(Roles = "Admin")]
    public class ClaimTypesModel : PageModel
    {
        private readonly IConfigurationDbContext _context;

        public ClaimTypesModel(IConfigurationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public IdentityResource IdentityResource { get; set; }

        public IEnumerable<IdentityClaim> ClaimTypes { get; set; }

        public async Task<IActionResult> OnGetAsync([FromRoute] int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            IdentityResource = await _context.IdentityResources
                .Include(x => x.UserClaims)
                .SingleOrDefaultAsync(x => x.Id.Equals(id))
                .ConfigureAwait(false);
            if (IdentityResource == null)
            {
                return NotFound();
            }

            ClaimTypes = IdentityResource.UserClaims
                .Select(x => new IdentityClaim
                {
                    Id = x.Id,
                    Type = x.Type
                });

            return Page();
        }
    }
}
