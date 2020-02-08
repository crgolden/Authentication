namespace Authentication.Pages.Roles
{
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Core.Entities;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;

    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<User> _userManager;

        public DeleteModel(
            RoleManager<Role> roleManager,
            UserManager<User> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [BindProperty]
        public Role Role { get; set; }

        public async Task<IActionResult> OnGetAsync([FromRoute] Guid id)
        {
            if (id.Equals(Guid.Empty))
            {
                return NotFound();
            }

            Role = await _roleManager.FindByIdAsync($"{id}").ConfigureAwait(false);
            if (Role == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Role.Id.Equals(Guid.Empty))
            {
                return Page();
            }

            var role = await _roleManager.FindByIdAsync($"{Role.Id}").ConfigureAwait(false);
            if (role == null)
            {
                return Page();
            }

            foreach (var user in await _userManager.GetUsersInRoleAsync(role.Name).ConfigureAwait(false))
            {
                await _userManager.RemoveClaimAsync(user, new Claim(ClaimTypes.Role, role.Name)).ConfigureAwait(false);
            }

            await _roleManager.DeleteAsync(role).ConfigureAwait(false);
            return RedirectToPage("./Index");
        }
    }
}
