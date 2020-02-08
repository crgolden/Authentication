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
    public class SecretsModel : PageModel
    {
        private readonly IConfigurationDbContext _context;

        public SecretsModel(IConfigurationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Client Client { get; set; }

        public IEnumerable<ClientSecret> Secrets { get; set; }

        public async Task<IActionResult> OnGetAsync([FromRoute] int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            Client = await _context.Clients
                .Include(x => x.ClientSecrets)
                .SingleOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);

            if (Client == null)
            {
                return NotFound();
            }

            Secrets = Client.ClientSecrets
                .Select(x => new ClientSecret
                {
                    Id = x.Id,
                    Type = x.Type,
                    Value = x.Value,
                    Description = x.Description,
                    Expiration = x.Expiration,
                    Created = x.Created
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
                .Include(x => x.ClientSecrets)
                .SingleOrDefaultAsync(x => x.Id == Client.Id)
                .ConfigureAwait(false);

            if (client == null)
            {
                return Page();
            }

            if (Client.ClientSecrets != null)
            {
                foreach (var clientSecret in Client.ClientSecrets.Where(x => x.Id > 0))
                {
                    var secret = client.ClientSecrets.Single(x => x.Id.Equals(clientSecret.Id));
                    secret.Type = clientSecret.Type;
                    secret.Value = clientSecret.Value;
                    secret.Description = clientSecret.Description;
                    secret.Expiration = clientSecret.Expiration;
                }

                client.ClientSecrets.AddRange(Client.ClientSecrets.Where(x => x.Id == 0).Select(x => new ClientSecret
                {
                    Type = x.Type,
                    Value = x.Value,
                    Description = x.Description,
                    Expiration = x.Expiration,
                    Created = DateTime.UtcNow
                }));
                var secrets = client.ClientSecrets.Where(x => !Client.ClientSecrets.Any(y => y.Id.Equals(x.Id))).ToArray();
                foreach (var secret in secrets)
                {
                    client.ClientSecrets.Remove(secret);
                }
            }
            else
            {
                client.ClientSecrets = new List<ClientSecret>();
            }

            client.Updated = DateTime.UtcNow;
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return RedirectToPage("../Details/Secrets", new { Client.Id });
        }
    }
}