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
                .SingleOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);

            if (ApiResource == null)
            {
                return NotFound();
            }

            Scopes = ApiResource.Scopes.Select(x => new ApiScope
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

        public async Task<IActionResult> OnPostAsync()
        {
            if (ApiResource.Id <= 0)
            {
                return Page();
            }

            var apiResource = await _context.ApiResources
                .Include(x => x.Scopes)
                .SingleOrDefaultAsync(x => x.Id == ApiResource.Id)
                .ConfigureAwait(false);

            if (apiResource == null)
            {
                return Page();
            }

            if (ApiResource.Scopes != null)
            {
                foreach (var apiResourceScope in ApiResource.Scopes.Where(x => x.Id > 0))
                {
                    var scope = apiResource.Scopes.Single(x => x.Id == apiResourceScope.Id);
                    scope.Name = apiResourceScope.Name;
                    scope.DisplayName = apiResourceScope.DisplayName;
                    scope.Description = apiResourceScope.Description;
                    scope.Emphasize = apiResourceScope.Emphasize;
                    scope.Required = apiResourceScope.Required;
                    scope.ShowInDiscoveryDocument = apiResourceScope.ShowInDiscoveryDocument;
                }

                apiResource.Scopes.AddRange(ApiResource.Scopes.Where(x => x.Id == 0));
                var scopes = apiResource.Scopes.Where(x => !ApiResource.Scopes.Any(y => y.Id.Equals(x.Id))).ToArray();
                foreach (var scope in scopes)
                {
                    apiResource.Scopes.Remove(scope);
                }
            }
            else
            {
                apiResource.Scopes = new List<ApiScope>();
            }

            apiResource.Updated = DateTime.UtcNow;
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return RedirectToPage("../Details/Scopes", new { ApiResource.Id });
        }
    }
}