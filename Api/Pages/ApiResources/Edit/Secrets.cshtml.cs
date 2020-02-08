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
                .SingleOrDefaultAsync(x => x.Id == id)
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

        public async Task<IActionResult> OnPostAsync()
        {
            if (ApiResource.Id <= 0)
            {
                return Page();
            }

            var apiResource = await _context.ApiResources
                .Include(x => x.Secrets)
                .SingleOrDefaultAsync(x => x.Id == ApiResource.Id)
                .ConfigureAwait(false);

            if (apiResource == null)
            {
                return Page();
            }

            if (ApiResource.Secrets != null)
            {
                foreach (var apiSecret in ApiResource.Secrets.Where(x => x.Id > 0))
                {
                    var secret = apiResource.Secrets.Single(x => x.Id.Equals(apiSecret.Id));
                    secret.Type = apiSecret.Type;
                    secret.Value = apiSecret.Value;
                    secret.Description = apiSecret.Description;
                    secret.Expiration = apiSecret.Expiration;
                }

                apiResource.Secrets.AddRange(ApiResource.Secrets.Where(x => x.Id == 0).Select(x => new ApiSecret
                {
                    Type = x.Type,
                    Value = x.Value,
                    Description = x.Description,
                    Expiration = x.Expiration,
                    Created = DateTime.UtcNow
                }));
                var secrets = apiResource.Secrets.Where(x => !ApiResource.Secrets.Any(y => y.Id.Equals(x.Id))).ToArray();
                foreach (var secret in secrets)
                {
                    apiResource.Secrets.Remove(secret);
                }
            }
            else
            {
                apiResource.Secrets = new List<ApiSecret>();
            }

            apiResource.Updated = DateTime.UtcNow;
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return RedirectToPage("../Details/Secrets", new { ApiResource.Id });
        }
    }
}