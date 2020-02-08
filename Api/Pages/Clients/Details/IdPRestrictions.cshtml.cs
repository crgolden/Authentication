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
    }
}
