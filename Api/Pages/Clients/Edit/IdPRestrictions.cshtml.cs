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
    public class IdPRestrictionsModel : PageModel
    {
        private readonly IConfigurationDbContext _context;

        public IdPRestrictionsModel(IConfigurationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Client Client { get; set; }

        public IEnumerable<ClientIdPRestriction> IdPRestrictions { get; set; }

        public async Task<IActionResult> OnGetAsync([FromRoute] int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            Client = await _context.Clients
                .Include(x => x.IdentityProviderRestrictions)
                .SingleOrDefaultAsync(x => x.Id.Equals(id))
                .ConfigureAwait(false);

            if (Client == null)
            {
                return NotFound();
            }

            IdPRestrictions = Client.IdentityProviderRestrictions
                .Select(x => new ClientIdPRestriction
                {
                    Id = x.Id,
                    Provider = x.Provider
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
                .Include(x => x.IdentityProviderRestrictions)
                .SingleOrDefaultAsync(x => x.Id.Equals(Client.Id))
                .ConfigureAwait(false);

            if (client == null)
            {
                return Page();
            }

            if (Client.IdentityProviderRestrictions != null)
            {
                foreach (var clientIdPRestriction in Client.IdentityProviderRestrictions.Where(x => x.Id > 0))
                {
                    var idPRestriction = client.IdentityProviderRestrictions.Single(x => x.Id.Equals(clientIdPRestriction.Id));
                    idPRestriction.Provider = clientIdPRestriction.Provider;
                }

                client.IdentityProviderRestrictions.AddRange(Client.IdentityProviderRestrictions.Where(x => x.Id == 0));
                var idPRestrictions = client.IdentityProviderRestrictions.Where(x => !Client.IdentityProviderRestrictions.Any(y => y.Id.Equals(x.Id))).ToArray();
                foreach (var idPRestriction in idPRestrictions)
                {
                    client.IdentityProviderRestrictions.Remove(idPRestriction);
                }
            }
            else
            {
                client.IdentityProviderRestrictions = new List<ClientIdPRestriction>();
            }

            client.Updated = DateTime.UtcNow;
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return RedirectToPage("../Details/IdPRestrictions", new { Client.Id });
        }
    }
}
