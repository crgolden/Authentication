namespace Authentication.Pages.IdentityResources.Edit
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

        public async Task<IActionResult> OnPostAsync()
        {
            if (IdentityResource.Id <= 0)
            {
                return Page();
            }

            var identityResource = await _context.IdentityResources
                .Include(x => x.Properties)
                .SingleOrDefaultAsync(x => x.Id.Equals(IdentityResource.Id))
                .ConfigureAwait(false);

            if (identityResource == null)
            {
                return Page();
            }

            if (IdentityResource.Properties != null)
            {
                foreach (var identityResourceProperty in IdentityResource.Properties.Where(x => x.Id > 0))
                {
                    var property = identityResource.Properties.Single(x => x.Id.Equals(identityResourceProperty.Id));
                    property.Key = identityResourceProperty.Key;
                    property.Value = identityResourceProperty.Value;
                }

                identityResource.Properties.AddRange(IdentityResource.Properties.Where(x => x.Id == 0));
                var properties = identityResource.Properties.Where(x => !IdentityResource.Properties.Any(y => y.Id.Equals(x.Id))).ToArray();
                foreach (var property in properties)
                {
                    identityResource.Properties.Remove(property);
                }
            }
            else
            {
                identityResource.Properties = new List<IdentityResourceProperty>();
            }

            identityResource.Updated = DateTime.UtcNow;
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return RedirectToPage("../Details/Properties", new { IdentityResource.Id });
        }
    }
}
