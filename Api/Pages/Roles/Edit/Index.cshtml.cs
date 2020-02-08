namespace Authentication.Pages.Roles.Edit
{
    using System;
    using System.Threading.Tasks;
    using Core.Entities;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;

    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly RoleManager<Role> _roleManager;

        public IndexModel(RoleManager<Role> roleManager)
        {
            _roleManager = roleManager;
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

            role.Name = Role.Name;

            await _roleManager.UpdateAsync(role).ConfigureAwait(false);
            return RedirectToPage("../Details/Index", new { Role.Id });
        }
    }
}
