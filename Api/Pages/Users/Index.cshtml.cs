namespace Authentication.Pages.Users
{
    using System.Collections.Generic;
    using Core.Entities;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc.RazorPages;

    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;

        public IndexModel(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public IEnumerable<User> Users { get; set; }

        public void OnGet()
        {
            Users = _userManager.Users;
        }
    }
}
