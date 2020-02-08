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
    public class GrantTypesModel : PageModel
    {
        private readonly IConfigurationDbContext _context;

        public GrantTypesModel(IConfigurationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Client Client { get; set; }

        public IEnumerable<ClientGrantType> GrantTypes { get; set; }

        public async Task<IActionResult> OnGetAsync([FromRoute] int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            Client = await _context.Clients
                .Include(x => x.AllowedGrantTypes)
                .SingleOrDefaultAsync(x => x.Id.Equals(id))
                .ConfigureAwait(false);

            if (Client == null)
            {
                return NotFound();
            }

            GrantTypes = Client.AllowedGrantTypes
                .Select(x => new ClientGrantType
                {
                    Id = x.Id,
                    GrantType = x.GrantType
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
                .Include(x => x.AllowedGrantTypes)
                .SingleOrDefaultAsync(x => x.Id.Equals(Client.Id))
                .ConfigureAwait(false);

            if (client == null)
            {
                return Page();
            }

            if (Client.AllowedGrantTypes != null)
            {
                foreach (var clientGrantType in Client.AllowedGrantTypes.Where(x => x.Id > 0))
                {
                    var grantType = client.AllowedGrantTypes.Single(x => x.Id.Equals(clientGrantType.Id));
                    grantType.GrantType = clientGrantType.GrantType;
                }

                client.AllowedGrantTypes.AddRange(Client.AllowedGrantTypes.Where(x => x.Id == 0));
                var grantTypes = client.AllowedGrantTypes.Where(x => !Client.AllowedGrantTypes.Any(y => y.Id.Equals(x.Id))).ToArray();
                foreach (var grantType in grantTypes)
                {
                    client.AllowedGrantTypes.Remove(grantType);
                }
            }
            else
            {
                client.AllowedGrantTypes = new List<ClientGrantType>();
            }

            client.Updated = DateTime.UtcNow;
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return RedirectToPage("../Details/GrantTypes", new { Client.Id });
        }
    }
}
