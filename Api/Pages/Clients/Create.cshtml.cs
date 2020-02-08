namespace Authentication.Pages.Clients
{
    using System;
    using System.Threading.Tasks;
    using IdentityServer4.EntityFramework.Entities;
    using IdentityServer4.EntityFramework.Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;

    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly IConfigurationDbContext _context;

        public CreateModel(IConfigurationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Client Client { get; set; }

        public IActionResult OnGet()
        {
            Client = new Client();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Client.Created = DateTime.UtcNow;
            _context.Clients.Add(Client);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return RedirectToPage("./Details/Index", new { Client.Id });
        }
    }
}