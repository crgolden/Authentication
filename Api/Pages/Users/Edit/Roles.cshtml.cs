namespace Authentication.Pages.Users.Edit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Core.Entities;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.EntityFrameworkCore;

    [Authorize(Roles = "Admin")]
    public class RolesModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;

        public RolesModel(
            UserManager<User> userManager,
            RoleManager<Role> roleManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [BindProperty]
        public User UserModel { get; set; }

        public IEnumerable<Role> Roles { get; set; }

        public async Task<IActionResult> OnGetAsync([FromRoute] Guid id)
        {
            if (id.Equals(Guid.Empty))
            {
                return NotFound();
            }

            UserModel = await _userManager.Users
                .Include(x => x.UserRoles)
                .SingleOrDefaultAsync(x => x.Id.Equals(id))
                .ConfigureAwait(false);

            if (UserModel == null)
            {
                return NotFound();
            }

            Roles = _roleManager.Roles.Select(x => new Role
            {
                Id = x.Id,
                Name = x.Name
            });

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (UserModel.Id.Equals(Guid.Empty))
            {
                return Page();
            }

            var user = await _userManager.Users
                .Include(x => x.UserRoles)
                .Include(x => x.Claims)
                .SingleOrDefaultAsync(x => x.Id.Equals(UserModel.Id))
                .ConfigureAwait(false);

            if (user == null)
            {
                return Page();
            }

            if (UserModel.UserRoles.Any())
            {
                foreach (var userRole in UserModel.UserRoles.Where(x => !user.UserRoles.Any(y => y.RoleId.Equals(x.RoleId))))
                {
                    var role = await _roleManager.FindByIdAsync($"{userRole.RoleId}").ConfigureAwait(false);
                    await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, role.Name)).ConfigureAwait(false);
                    user.UserRoles.Add(userRole);
                }

                var userRoles = user.UserRoles.Where(x => !UserModel.UserRoles.Any(y => y.RoleId.Equals(x.RoleId))).ToArray();
                foreach (var userRole in userRoles)
                {
                    var role = await _roleManager.FindByIdAsync($"{userRole.RoleId}").ConfigureAwait(false);
                    await _userManager.RemoveClaimAsync(user, new Claim(ClaimTypes.Role, role.Name)).ConfigureAwait(false);
                    user.UserRoles.Remove(userRole);
                }
            }
            else
            {
                user.UserRoles.Clear();
                await _userManager.RemoveClaimsAsync(user, user.Claims
                    .Where(x => x.ClaimType.Equals(ClaimTypes.Role))
                    .Select(x => x.ToClaim()))
                    .ConfigureAwait(false);
            }

            await _userManager.UpdateAsync(user).ConfigureAwait(false);
            return RedirectToPage("../Details/Roles", new { UserModel.Id });
        }
    }
}
