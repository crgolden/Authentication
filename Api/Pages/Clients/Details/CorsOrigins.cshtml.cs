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
    }
}
