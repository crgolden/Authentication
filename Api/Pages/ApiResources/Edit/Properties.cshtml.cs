namespace Authentication.Pages.ApiResources.Edit
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
        public ApiResource ApiResource { get; set; }

        public IEnumerable<ApiResourceProperty> Properties { get; set; }

        public async Task<IActionResult> OnGetAsync([FromRoute] int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            ApiResource = await _context.ApiResources
                .Include(x => x.Properties)
                .SingleOrDefaultAsync(x => x.Id.Equals(id))
                .ConfigureAwait(false);

            if (ApiResource == null)
            {
                return NotFound();
            }

            Properties = ApiResource.Properties
                .Select(x => new ApiResourceProperty
                {
                    Id = x.Id,
                    Key = x.Key,
                    Value = x.Value
                });

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ApiResource.Id <= 0)
            {
                return Page();
            }

            var apiResource = await _context.ApiResources
                .Include(x => x.Properties)
                .SingleOrDefaultAsync(x => x.Id.Equals(ApiResource.Id))
                .ConfigureAwait(false);

            if (apiResource == null)
            {
                return Page();
            }

            if (ApiResource.Properties != null)
            {
                foreach (var apiResourceProperty in ApiResource.Properties.Where(x => x.Id > 0))
                {
                    var property = apiResource.Properties.Single(x => x.Id.Equals(apiResourceProperty.Id));
                    property.Key = apiResourceProperty.Key;
                    property.Value = apiResourceProperty.Value;
                }

                apiResource.Properties.AddRange(ApiResource.Properties.Where(x => x.Id == 0));
                var properties = apiResource.Properties.Where(x => !ApiResource.Properties.Any(y => y.Id.Equals(x.Id))).ToArray();
                foreach (var property in properties)
                {
                    apiResource.Properties.Remove(property);
                }
            }
            else
            {
                apiResource.Properties = new List<ApiResourceProperty>();
            }

            apiResource.Updated = DateTime.UtcNow;
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return RedirectToPage("../Details/Properties", new { ApiResource.Id });
        }
    }
}
