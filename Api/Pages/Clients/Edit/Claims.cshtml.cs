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
    public class ClaimsModel : PageModel
    {
        private readonly IConfigurationDbContext _context;

        public ClaimsModel(IConfigurationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Client Client { get; set; }

        public IEnumerable<ClientClaim> Claims { get; set; }

        public async Task<IActionResult> OnGetAsync([FromRoute] int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            Client = await _context.Clients
                .Include(x => x.Claims)
                .SingleOrDefaultAsync(x => x.Id.Equals(id))
                .ConfigureAwait(false);

            if (Client == null)
            {
                return NotFound();
            }

            Claims = Client.Claims
                .Select(x => new ClientClaim
                {
                    Id = x.Id,
                    Type = x.Type,
                    Value = x.Value
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
                .Include(x => x.Claims)
                .SingleOrDefaultAsync(x => x.Id.Equals(Client.Id))
                .ConfigureAwait(false);

            if (client == null)
            {
                return Page();
            }

            client.Updated = DateTime.UtcNow;
            if (Client.Claims != null)
            {
                foreach (var clientClaim in Client.Claims.Where(x => x.Id > 0))
                {
                    var claim = client.Claims.Single(x => x.Id.Equals(clientClaim.Id));
                    claim.Type = clientClaim.Type;
                    claim.Value = clientClaim.Value;
                }

                client.Claims.AddRange(Client.Claims.Where(x => x.Id == 0));
                var claims = client.Claims.Where(x => !Client.Claims.Any(y => y.Id.Equals(x.Id))).ToArray();
                foreach (var claim in claims)
                {
                    client.Claims.Remove(claim);
                }
            }
            else
            {
                client.Claims = new List<ClientClaim>();
            }

            await _context.SaveChangesAsync().ConfigureAwait(false);
            return RedirectToPage("../Details/Claims", new { Client.Id });
        }
    }
}
