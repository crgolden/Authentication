namespace Authentication.Pages.IdentityResources.Details
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
    public class PropertiesModel : PageModel
    {
        private readonly IConfigurationDbContext _context;

        public PropertiesModel(IConfigurationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public IdentityResource IdentityResource { get; set; }

        public IEnumerable<IdentityResourceProperty> Properties { get; set; }

        public async Task<IActionResult> OnGetAsync([FromRoute] int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            IdentityResource = await _context.IdentityResources
                .Include(x => x.Properties)
                .SingleOrDefaultAsync(x => x.Id.Equals(id))
                .ConfigureAwait(false);
            if (IdentityResource == null)
            {
                return NotFound();
            }

            Properties = IdentityResource.Properties
                .Select(x => new IdentityResourceProperty
                {
                    Id = x.Id,
                    Key = x.Key,
                    Value = x.Value
                });

            return Page();
        }
    }
}
