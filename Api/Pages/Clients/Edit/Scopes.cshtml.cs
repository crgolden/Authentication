namespace Authentication.Pages.Clients.Edit
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
    public class ScopesModel : PageModel
    {
        private readonly IConfigurationDbContext _context;

        public ScopesModel(IConfigurationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Client Client { get; set; }

        public IEnumerable<ClientScope> Scopes { get; set; }

        public async Task<IActionResult> OnGetAsync([FromRoute] int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            Client = await _context.Clients
                .Include(x => x.AllowedScopes)
                .SingleOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);

            if (Client == null)
            {
                return NotFound();
            }

            Scopes = Client.AllowedScopes.Select(x => new ClientScope
                {
                    Id = x.Id,
                    Scope = x.Scope
                });

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Client.Id <= 0)
            {
                return Page();
            }

            var client = await _context.Clients
                .Include(x => x.AllowedScopes)
                .SingleOrDefaultAsync(x => x.Id == Client.Id)
                .ConfigureAwait(false);

            if (client == null)
            {
                return Page();
            }

            if (Client.AllowedScopes != null)
            {
                foreach (var clientScope in Client.AllowedScopes.Where(x => x.Id > 0))
                {
                    var scope = client.AllowedScopes.Single(x => x.Id == clientScope.Id);
                    scope.Scope = clientScope.Scope;
                }

                client.AllowedScopes.AddRange(Client.AllowedScopes.Where(x => x.Id == 0));
                var scopes = client.AllowedScopes.Where(x => !Client.AllowedScopes.Any(y => y.Id.Equals(x.Id))).ToArray();
                foreach (var scope in scopes)
                {
                    client.AllowedScopes.Remove(scope);
                }
            }
            else
            {
                client.AllowedScopes = new List<ClientScope>();
            }

            client.Updated = DateTime.UtcNow;
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return RedirectToPage("../Details/Scopes", new { Client.Id });
        }
    }
}