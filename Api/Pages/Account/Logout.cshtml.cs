namespace Authentication.Pages.Account
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using IdentityModel;
    using IdentityServer4;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(IIdentityServerInteractionService interaction, ILogger<LogoutModel> logger)
        {
            _interaction = interaction;
            _logger = logger;
        }

        public string LogoutId { get; set; }

        [TempData]
        [ViewData]
        public string Origin { get; set; }

        public async Task<IActionResult> OnGetAsync(string logoutId)
        {
            if (User.Identity.IsAuthenticated == false)
            {
                return await OnPostAsync(logoutId).ConfigureAwait(false);
            }

            var context = await _interaction.GetLogoutContextAsync(logoutId).ConfigureAwait(false);
            if (context?.ShowSignoutPrompt == false)
            {
                return await OnPostAsync(logoutId).ConfigureAwait(false);
            }

            LogoutId = logoutId;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string logoutId)
        {
            var idp = User?.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
            if (!string.IsNullOrWhiteSpace(idp) && idp != IdentityServerConstants.LocalIdentityProvider)
            {
                var provider = HttpContext.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();
                var handler = await provider.GetHandlerAsync(HttpContext, idp).ConfigureAwait(false);
                if (handler is IAuthenticationSignOutHandler)
                {
                    if (string.IsNullOrEmpty(logoutId))
                    {
                        logoutId = await _interaction.CreateLogoutContextAsync().ConfigureAwait(false);
                    }

                    await HttpContext.SignOutAsync(idp, new AuthenticationProperties
                    {
                        RedirectUri = $"/account/logout?logoutId={logoutId}"
                    }).ConfigureAwait(false);
                }
            }

            await HttpContext.SignOutAsync().ConfigureAwait(false);
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme).ConfigureAwait(false);
            _logger.LogInformation("User logged out.");
            HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
            var logout = await _interaction.GetLogoutContextAsync(logoutId).ConfigureAwait(false);
            return Redirect(logout?.PostLogoutRedirectUri ?? Url.Content("~/"));
        }
    }
}
