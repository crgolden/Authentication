namespace Authentication.Pages.ApiResources.Details
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
    public class ScopesModel : PageModel
    {
        private readonly IConfigurationDbContext _context;

        public ScopesModel(IConfigurationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ApiResource ApiResource { get; set; }

        public IEnumerable<ApiScope> Scopes { get; set; }

        public async Task<IActionResult> OnGetAsync([FromRoute] int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            ApiResource = await _context.ApiResources
                .Include(x => x.Scopes)
                .SingleOrDefaultAsync(x => x.Id.Equals(id))
                .ConfigureAwait(false);
            if (ApiResource == null)
            {
                return NotFound();
            }

            Scopes = ApiResource.Scopes
                .Select(x => new ApiScope
                {
                    Id = x.Id,
                    Name = x.Name,
                    DisplayName = x.DisplayName,
                    Description = x.Description,
                    Emphasize = x.Emphasize,
                    Required = x.Required,
                    ShowInDiscoveryDocument = x.ShowInDiscoveryDocument
                });

            return Page();
        }
    }
}
