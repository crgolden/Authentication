namespace Authentication.Pages.Clients.Details
{
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
    }
}
