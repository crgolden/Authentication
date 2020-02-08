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
    public class SecretsModel : PageModel
    {
        private readonly IConfigurationDbContext _context;

        public SecretsModel(IConfigurationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ApiResource ApiResource { get; set; }

        public IEnumerable<ApiSecret> Secrets { get; set; }

        public async Task<IActionResult> OnGetAsync([FromRoute] int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            ApiResource = await _context.ApiResources
                .Include(x => x.Secrets)
                .SingleOrDefaultAsync(x => x.Id.Equals(id))
                .ConfigureAwait(false);
            if (ApiResource == null)
            {
                return NotFound();
            }

            Secrets = ApiResource.Secrets
                .Select(x => new ApiSecret
                {
                    Id = x.Id,
                    Type = x.Type,
                    Value = x.Value,
                    Description = x.Description,
                    Expiration = x.Expiration,
                    Created = x.Created
                });

            return Page();
        }
    }
}
