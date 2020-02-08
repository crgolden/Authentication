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
    }
}
