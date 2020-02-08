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
    public class CorsOriginsModel : PageModel
    {
        private readonly IConfigurationDbContext _context;

        public CorsOriginsModel(IConfigurationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Client Client { get; set; }

        public IEnumerable<ClientCorsOrigin> CorsOrigins { get; set; }

        public async Task<IActionResult> OnGetAsync([FromRoute] int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            Client = await _context.Clients
                .Include(x => x.AllowedCorsOrigins)
                .SingleOrDefaultAsync(x => x.Id.Equals(id))
                .ConfigureAwait(false);

            if (Client == null)
            {
                return NotFound();
            }

            CorsOrigins = Client.AllowedCorsOrigins
                .Select(x => new ClientCorsOrigin
                {
                    Id = x.Id,
                    Origin = x.Origin
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
                .Include(x => x.AllowedCorsOrigins)
                .SingleOrDefaultAsync(x => x.Id.Equals(Client.Id))
                .ConfigureAwait(false);

            if (client == null)
            {
                return Page();
            }

            if (Client.AllowedCorsOrigins != null)
            {
                foreach (var clientCorsOrigin in Client.AllowedCorsOrigins.Where(x => x.Id > 0))
                {
                    var corsOrigin = client.AllowedCorsOrigins.Single(x => x.Id.Equals(clientCorsOrigin.Id));
                    corsOrigin.Origin = clientCorsOrigin.Origin;
                }

                client.AllowedCorsOrigins.AddRange(Client.AllowedCorsOrigins.Where(x => x.Id == 0));
                var corsOrigins = client.AllowedCorsOrigins.Where(x => !Client.AllowedCorsOrigins.Any(y => y.Id.Equals(x.Id))).ToArray();
                foreach (var corsOrigin in corsOrigins)
                {
                    client.AllowedCorsOrigins.Remove(corsOrigin);
                }
            }
            else
            {
                client.AllowedCorsOrigins = new List<ClientCorsOrigin>();
            }

            client.Updated = DateTime.UtcNow;
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return RedirectToPage("../Details/CorsOrigins", new { Client.Id });
        }
    }
}
