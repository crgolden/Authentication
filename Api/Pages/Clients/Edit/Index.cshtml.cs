namespace Authentication.Pages.Clients.Edit
{
    using System;
    using System.Threading.Tasks;
    using IdentityServer4.EntityFramework.Entities;
    using IdentityServer4.EntityFramework.Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;

    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IConfigurationDbContext _context;

        public IndexModel(IConfigurationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Client Client { get; set; }

        public async Task<IActionResult> OnGetAsync([FromRoute] int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            Client = await _context.Clients.FindAsync(id).ConfigureAwait(false);
            if (Client == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Client.Id <= 0)
            {
                return Page();
            }

            var client = await _context.Clients.FindAsync(Client.Id).ConfigureAwait(false);
            if (client == null)
            {
                return Page();
            }

            client.AbsoluteRefreshTokenLifetime = Client.AbsoluteRefreshTokenLifetime;
            client.AccessTokenLifetime = Client.AccessTokenLifetime;
            client.AccessTokenType = Client.AccessTokenType;
            client.AllowAccessTokensViaBrowser = Client.AllowAccessTokensViaBrowser;
            client.AllowOfflineAccess = Client.AllowOfflineAccess;
            client.AllowPlainTextPkce = Client.AllowPlainTextPkce;
            client.AllowRememberConsent = Client.AllowRememberConsent;
            client.AlwaysIncludeUserClaimsInIdToken = Client.AlwaysIncludeUserClaimsInIdToken;
            client.AlwaysSendClientClaims = Client.AlwaysSendClientClaims;
            client.AuthorizationCodeLifetime = Client.AuthorizationCodeLifetime;
            client.BackChannelLogoutSessionRequired = Client.BackChannelLogoutSessionRequired;
            client.BackChannelLogoutUri = Client.BackChannelLogoutUri;
            client.ClientClaimsPrefix = Client.ClientClaimsPrefix;
            client.ClientId = Client.ClientId;
            client.ClientName = Client.ClientName;
            client.ClientUri = Client.ClientUri;
            client.ConsentLifetime = Client.ConsentLifetime;
            client.Description = Client.Description;
            client.DeviceCodeLifetime = Client.DeviceCodeLifetime;
            client.EnableLocalLogin = Client.EnableLocalLogin;
            client.Enabled = Client.Enabled;
            client.FrontChannelLogoutSessionRequired = Client.FrontChannelLogoutSessionRequired;
            client.FrontChannelLogoutUri = Client.FrontChannelLogoutUri;
            client.IdentityTokenLifetime = Client.IdentityTokenLifetime;
            client.IncludeJwtId = Client.IncludeJwtId;
            client.LogoUri = Client.LogoUri;
            client.NonEditable = Client.NonEditable;
            client.PairWiseSubjectSalt = Client.PairWiseSubjectSalt;
            client.ProtocolType = Client.ProtocolType;
            client.RefreshTokenExpiration = Client.RefreshTokenExpiration;
            client.RefreshTokenUsage = Client.RefreshTokenUsage;
            client.RequireClientSecret = Client.RequireClientSecret;
            client.RequireConsent = Client.RequireConsent;
            client.RequirePkce = Client.RequirePkce;
            client.SlidingRefreshTokenLifetime = Client.SlidingRefreshTokenLifetime;
            client.UpdateAccessTokenClaimsOnRefresh = Client.UpdateAccessTokenClaimsOnRefresh;
            client.Updated = DateTime.UtcNow;
            client.UserCodeType = Client.UserCodeType;
            client.UserSsoLifetime = Client.UserSsoLifetime;

            await _context.SaveChangesAsync().ConfigureAwait(false);
            return RedirectToPage("../Details/Index", new { Client.Id });
        }
    }
}
