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
    public class PropertiesModel : PageModel
    {
        private readonly IConfigurationDbContext _context;

        public PropertiesModel(IConfigurationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Client Client { get; set; }

        public IEnumerable<ClientProperty> Properties { get; set; }

        public async Task<IActionResult> OnGetAsync([FromRoute] int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            Client = await _context.Clients
                .Include(x => x.Properties)
                .SingleOrDefaultAsync(x => x.Id.Equals(id))
                .ConfigureAwait(false);

            if (Client == null)
            {
                return NotFound();
            }

            Properties = Client.Properties
                .Select(x => new ClientProperty
                {
                    Id = x.Id,
                    Key = x.Key,
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
                .Include(x => x.Properties)
                .SingleOrDefaultAsync(x => x.Id.Equals(Client.Id))
                .ConfigureAwait(false);

            if (client == null)
            {
                return Page();
            }

            if (Client.Properties != null)
            {
                foreach (var clientProperty in Client.Properties.Where(x => x.Id > 0))
                {
                    var property = client.Properties.Single(x => x.Id.Equals(clientProperty.Id));
                    property.Key = clientProperty.Key;
                    property.Value = clientProperty.Value;
                }

                client.Properties.AddRange(Client.Properties.Where(x => x.Id == 0));
                var properties = client.Properties.Where(x => !Client.Properties.Any(y => y.Id.Equals(x.Id))).ToArray();
                foreach (var property in properties)
                {
                    client.Properties.Remove(property);
                }
            }
            else
            {
                client.Properties = new List<ClientProperty>();
            }

            client.Updated = DateTime.UtcNow;
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return RedirectToPage("../Details/Properties", new { Client.Id });
        }
    }
}
