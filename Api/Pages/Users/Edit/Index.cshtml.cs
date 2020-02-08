namespace Authentication.Pages.Users.Edit
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
        private readonly UserManager<User> _userManager;

        public IndexModel(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public User UserModel { get; set; }

        public async Task<IActionResult> OnGetAsync([FromRoute] Guid id)
        {
            if (id.Equals(Guid.Empty))
            {
                return NotFound();
            }

            UserModel = await _userManager.FindByIdAsync($"{id}").ConfigureAwait(false);
            if (UserModel == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (UserModel.Id.Equals(Guid.Empty))
            {
                return Page();
            }

            var user = await _userManager.FindByIdAsync($"{UserModel.Id}").ConfigureAwait(false);
            if (user == null)
            {
                return Page();
            }

            user.EmailConfirmed = UserModel.EmailConfirmed;
            user.TwoFactorEnabled = UserModel.TwoFactorEnabled;
            user.PhoneNumberConfirmed = UserModel.PhoneNumberConfirmed;
            user.LockoutEnabled = UserModel.LockoutEnabled;
            user.Email = UserModel.Email;
            user.UserName = UserModel.Email;
            user.PhoneNumber = UserModel.PhoneNumber;
            user.AccessFailedCount = UserModel.AccessFailedCount;

            await _userManager.UpdateAsync(user).ConfigureAwait(false);
            return RedirectToPage("../Details/Index", new { UserModel.Id });
        }
    }
}
