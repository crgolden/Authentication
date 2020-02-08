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
    public class RedirectUrisModel : PageModel
    {
        private readonly IConfigurationDbContext _context;

        public RedirectUrisModel(IConfigurationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Client Client { get; set; }

        public IEnumerable<ClientRedirectUri> RedirectUris { get; set; }

        public async Task<IActionResult> OnGetAsync([FromRoute] int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            Client = await _context.Clients
                .Include(x => x.RedirectUris)
                .SingleOrDefaultAsync(x => x.Id.Equals(id))
                .ConfigureAwait(false);

            if (Client == null)
            {
                return NotFound();
            }

            RedirectUris = Client.RedirectUris
                .Select(x => new ClientRedirectUri
                {
                    Id = x.Id,
                    RedirectUri = x.RedirectUri
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
                .Include(x => x.RedirectUris)
                .SingleOrDefaultAsync(x => x.Id.Equals(Client.Id))
                .ConfigureAwait(false);

            if (client == null)
            {
                return Page();
            }

            if (Client.RedirectUris != null)
            {
                foreach (var clientRedirectUri in Client.RedirectUris.Where(x => x.Id > 0))
                {
                    var redirectUri = client.RedirectUris.Single(x => x.Id.Equals(clientRedirectUri.Id));
                    redirectUri.RedirectUri = clientRedirectUri.RedirectUri;
                }

                client.RedirectUris.AddRange(Client.RedirectUris.Where(x => x.Id == 0));
                var redirectUris = client.RedirectUris.Where(x => !Client.RedirectUris.Any(y => y.Id.Equals(x.Id))).ToArray();
                foreach (var redirectUri in redirectUris)
                {
                    client.RedirectUris.Remove(redirectUri);
                }
            }
            else
            {
                client.RedirectUris = new List<ClientRedirectUri>();
            }

            client.Updated = DateTime.UtcNow;
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return RedirectToPage("../Details/RedirectUris", new { Client.Id });
        }
    }
}
