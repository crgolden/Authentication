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
    public class PostLogoutRedirectUrisModel : PageModel
    {
        private readonly IConfigurationDbContext _context;

        public PostLogoutRedirectUrisModel(IConfigurationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Client Client { get; set; }

        public IEnumerable<ClientPostLogoutRedirectUri> PostLogoutRedirectUris { get; set; }

        public async Task<IActionResult> OnGetAsync([FromRoute] int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            Client = await _context.Clients
                .Include(x => x.PostLogoutRedirectUris)
                .SingleOrDefaultAsync(x => x.Id.Equals(id))
                .ConfigureAwait(false);

            if (Client == null)
            {
                return NotFound();
            }

            PostLogoutRedirectUris = Client.PostLogoutRedirectUris
                .Select(x => new ClientPostLogoutRedirectUri
                {
                    Id = x.Id,
                    PostLogoutRedirectUri = x.PostLogoutRedirectUri
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
                .Include(x => x.PostLogoutRedirectUris)
                .SingleOrDefaultAsync(x => x.Id.Equals(Client.Id))
                .ConfigureAwait(false);

            if (client == null)
            {
                return Page();
            }

            if (Client.PostLogoutRedirectUris != null)
            {
                foreach (var clientPostLogoutRedirectUri in Client.PostLogoutRedirectUris.Where(x => x.Id > 0))
                {
                    var postLogoutRedirectUri = client.PostLogoutRedirectUris.Single(x => x.Id.Equals(clientPostLogoutRedirectUri.Id));
                    postLogoutRedirectUri.PostLogoutRedirectUri = clientPostLogoutRedirectUri.PostLogoutRedirectUri;
                }

                client.PostLogoutRedirectUris.AddRange(Client.PostLogoutRedirectUris.Where(x => x.Id == 0));
                var postLogoutRedirectUris = client.PostLogoutRedirectUris.Where(x => !Client.PostLogoutRedirectUris.Any(y => y.Id.Equals(x.Id))).ToArray();
                foreach (var postLogoutRedirectUri in postLogoutRedirectUris)
                {
                    client.PostLogoutRedirectUris.Remove(postLogoutRedirectUri);
                }
            }
            else
            {
                client.PostLogoutRedirectUris = new List<ClientPostLogoutRedirectUri>();
            }

            client.Updated = DateTime.UtcNow;
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return RedirectToPage("../Details/PostLogoutRedirectUris", new { Client.Id });
        }
    }
}
