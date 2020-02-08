namespace Authentication.Pages.Roles.Edit
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
    public class UsersModel : PageModel
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<User> _userManager;

        public UsersModel(
            RoleManager<Role> roleManager,
            UserManager<User> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [BindProperty]
        public Role Role { get; set; }

        public IEnumerable<User> Users { get; set; }

        public async Task<IActionResult> OnGetAsync([FromRoute] Guid id)
        {
            if (id.Equals(Guid.Empty))
            {
                return NotFound();
            }

            Role = await _roleManager.Roles
                .Include(x => x.UserRoles)
                .SingleOrDefaultAsync(x => x.Id.Equals(id))
                .ConfigureAwait(false);

            if (Role == null)
            {
                return NotFound();
            }

            Users = _userManager.Users.Select(x => new User
            {
                Id = x.Id,
                Email = x.Email
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
                .Include(x => x.UserRoles)
                .SingleOrDefaultAsync(x => x.Id.Equals(Role.Id))
                .ConfigureAwait(false);

            if (role == null)
            {
                return Page();
            }

            if (Role.UserRoles.Any())
            {
                foreach (var roleUser in Role.UserRoles.Where(x => !role.UserRoles.Any(y => y.UserId.Equals(x.UserId))))
                {
                    var user = await _userManager.FindByIdAsync($"{roleUser.UserId}").ConfigureAwait(false);
                    await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, role.Name)).ConfigureAwait(false);
                    role.UserRoles.Add(roleUser);
                }

                var roleUsers = role.UserRoles.Where(x => !Role.UserRoles.Any(y => y.UserId.Equals(x.UserId))).ToArray();
                foreach (var roleUser in roleUsers)
                {
                    var user = await _userManager.FindByIdAsync($"{roleUser.UserId}").ConfigureAwait(false);
                    await _userManager.RemoveClaimAsync(user, new Claim(ClaimTypes.Role, role.Name)).ConfigureAwait(false);
                    role.UserRoles.Remove(roleUser);
                }
            }
            else
            {
                role.UserRoles.Clear();
                var users = await _userManager.GetUsersInRoleAsync(role.Name).ConfigureAwait(false);
                foreach (var user in users)
                {
                    await _userManager.RemoveClaimAsync(user, new Claim(ClaimTypes.Role, role.Name)).ConfigureAwait(false);
                }
            }

            await _roleManager.UpdateAsync(role).ConfigureAwait(false);
            return RedirectToPage("../Details/Users", new { Role.Id });
        }
    }
}
