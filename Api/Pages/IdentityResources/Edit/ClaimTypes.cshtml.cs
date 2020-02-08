namespace Authentication.Pages.IdentityResources.Edit
{
    using System;
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

        public async Task<IActionResult> OnPostAsync()
        {
            if (IdentityResource.Id <= 0)
            {
                return Page();
            }

            var identityResource = await _context.IdentityResources
                .Include(x => x.UserClaims)
                .SingleOrDefaultAsync(x => x.Id.Equals(IdentityResource.Id))
                .ConfigureAwait(false);

            if (identityResource == null)
            {
                return Page();
            }

            if (IdentityResource.UserClaims != null)
            {
                foreach (var identityResourceClaimType in IdentityResource.UserClaims.Where(x => x.Id > 0))
                {
                    var claimType = identityResource.UserClaims.Single(x => x.Id.Equals(identityResourceClaimType.Id));
                    claimType.Type = identityResourceClaimType.Type;
                }

                identityResource.UserClaims.AddRange(IdentityResource.UserClaims.Where(x => x.Id == 0));
                var claims = identityResource.UserClaims.Where(x => !IdentityResource.UserClaims.Any(y => y.Id.Equals(x.Id))).ToArray();
                foreach (var claim in claims)
                {
                    identityResource.UserClaims.Remove(claim);
                }
            }
            else
            {
                identityResource.UserClaims = new List<IdentityClaim>();
            }

            identityResource.Updated = DateTime.UtcNow;
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return RedirectToPage("../Details/ClaimTypes", new { IdentityResource.Id });
        }
    }
}
