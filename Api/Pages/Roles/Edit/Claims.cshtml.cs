namespace Authentication.Pages.Roles.Edit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.Entities;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.EntityFrameworkCore;

    [Authorize(Roles = "Admin")]
    public class ClaimsModel : PageModel
    {
        private readonly RoleManager<Role> _roleManager;

        public ClaimsModel(RoleManager<Role> roleManager)
        {
            _roleManager = roleManager;
        }

        [BindProperty]
        public Role Role { get; set; }

        public IEnumerable<RoleClaim> Claims { get; set; }

        public async Task<IActionResult> OnGetAsync([FromRoute] Guid id)
        {
            if (id.Equals(Guid.Empty))
            {
                return NotFound();
            }

            Role = await _roleManager.Roles
                .Include(x => x.RoleClaims)
                .SingleOrDefaultAsync(x => x.Id.Equals(id))
                .ConfigureAwait(false);

            if (Role == null)
            {
                return NotFound();
            }

            Claims = Role.RoleClaims
                .Select(x => new RoleClaim
                {
                    Id = x.Id,
                    ClaimType = x.ClaimType,
                    ClaimValue = x.ClaimValue
                });

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Role.Id.Equals(Guid.Empty))
            {
                return Page();
            }

            var role = await _roleManager.Roles
                .Include(x => x.RoleClaims)
                .SingleOrDefaultAsync(x => x.Id.Equals(Role.Id))
                .ConfigureAwait(false);

            if (role == null)
            {
                return Page();
            }

            if (Role.RoleClaims != null)
            {
                foreach (var roleClaim in Role.RoleClaims.Where(x => x.Id > 0))
                {
                    var claim = role.RoleClaims.Single(x => x.Id.Equals(roleClaim.Id));
                    if (claim.ClaimType.Equals(roleClaim.ClaimType) && claim.ClaimValue.Equals(roleClaim.ClaimValue))
                    {
                        continue;
                    }

                    await _roleManager.RemoveClaimAsync(role, claim.ToClaim()).ConfigureAwait(false);
                    await _roleManager.AddClaimAsync(role, roleClaim.ToClaim()).ConfigureAwait(false);
                }

                foreach (var roleClaim in Role.RoleClaims.Where(x => x.Id == 0))
                {
                    await _roleManager.AddClaimAsync(role, roleClaim.ToClaim()).ConfigureAwait(false);
                }

                var roleClaims = role.RoleClaims.Where(x => !Role.RoleClaims.Any(y => y.Id.Equals(x.Id))).ToArray();
                foreach (var roleClaim in roleClaims)
                {
                    await _roleManager.RemoveClaimAsync(role, roleClaim.ToClaim()).ConfigureAwait(false);
                }
            }
            else
            {
                var roleClaims = role.RoleClaims.ToArray();
                foreach (var roleClaim in roleClaims)
                {
                    await _roleManager.RemoveClaimAsync(role, roleClaim.ToClaim()).ConfigureAwait(false);
                }
            }

            return RedirectToPage("../Details/Claims", new { Role.Id });
        }
    }
}
