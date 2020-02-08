namespace Authentication.Pages.Account
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Security.Claims;
    using System.Text;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Core.Entities;
    using IdentityModel;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Core;
    using Microsoft.Extensions.Logging;
    using static Microsoft.Net.Http.Headers.HeaderNames;

    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ISenderClient _senderClient;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            ISenderClient senderClient,
            ILogger<ExternalLoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _senderClient = senderClient;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string LoginProvider { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        [TempData]
        [ViewData]
        public string Origin { get; set; }

        public class InputModel
        {
            //[Required]
            [EmailAddress]
            public string Email { get; set; }

            //[Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            //[Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }
        }

        public IActionResult OnGetAsync(string returnUrl = null)
        {
            TempData[nameof(Origin)] = Origin;
            returnUrl ??= Url.Content("~/");

            return RedirectToPage("./Login", new { returnUrl });
        }

        public IActionResult OnPost(string provider, string returnUrl = null, string origin = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page(
                pageName: "./ExternalLogin",
                pageHandler: "Callback",
                values: new { returnUrl, origin });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(
                provider: provider,
                redirectUrl: redirectUrl);
            if (!string.IsNullOrEmpty(origin))
            {
                properties.Items.Add(Referer, $"{origin}");
            }

            return new ChallengeResult(
                authenticationScheme: provider,
                properties: properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string origin = null, string remoteError = null)
        {
            returnUrl ??= Url.Content("~/");
            TempData[nameof(Origin)] = origin;

            if (!string.IsNullOrEmpty(remoteError))
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { returnUrl });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync().ConfigureAwait(false);
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { returnUrl });
            }

            var user = await _userManager.FindByLoginAsync(
                loginProvider: info.LoginProvider,
                providerKey: info.ProviderKey).ConfigureAwait(false);
            if (user != null)
            {
                if (returnUrl == Url.Content("~/") && !await _userManager.IsInRoleAsync(user, "Admin").ConfigureAwait(false))
                {
                    return RedirectToPage("./Login");
                }

                // Sign in the user with this external login provider if the user already has a login.
                var result = await _signInManager.ExternalLoginSignInAsync(
                    loginProvider: info.LoginProvider,
                    providerKey: info.ProviderKey,
                    isPersistent: false,
                    bypassTwoFactor: true).ConfigureAwait(false);
                if (result.Succeeded)
                {
                    _logger.LogInformation($"User account with email '{user.Email}' logged in with {info.LoginProvider} provider.");
                    return LocalRedirect(returnUrl);
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning($"User account with email '{user.Email}' locked out.");
                    return RedirectToPage("./Lockout");
                }

                if (result.IsNotAllowed)
                {
                    return RedirectToPage("./VerifyEmail", new { user.Email, returnUrl });
                }
            }
            else if (info.Principal.HasClaim(x => x.Type == ClaimTypes.Email))
            {
                user = await _userManager.FindByEmailAsync(info.Principal.FindFirstValue(ClaimTypes.Email)).ConfigureAwait(false);
                if (user != null)
                {
                    var result = await _userManager.AddLoginAsync(
                        user: user,
                        login: info).ConfigureAwait(false);
                    if (result.Succeeded)
                    {
                        return await UpdateUser(
                            user: user,
                            info: info,
                            returnUrl: returnUrl).ConfigureAwait(false);
                    }
                }
            }

            // If the user does not have an account, then ask the user to create an account.
            Input = new InputModel();

            if (info.Principal.HasClaim(x => x.Type == ClaimTypes.Email))
            {
                Input.Email = info.Principal.FindFirstValue(ClaimTypes.Email);
            }

            if (info.Principal.HasClaim(x => x.Type == ClaimTypes.GivenName))
            {
                Input.FirstName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
            }

            if (info.Principal.HasClaim(x => x.Type == ClaimTypes.Surname))
            {
                Input.LastName = info.Principal.FindFirstValue(ClaimTypes.Surname);
            }

            LoginProvider = info.LoginProvider;
            Origin = origin;
            ReturnUrl = returnUrl;
            return Page();
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null, string origin = null)
        {
            returnUrl ??= Url.Content("~/");
            TempData[nameof(Origin)] = origin;

            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync().ConfigureAwait(false);
            if (info == null)
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("./Login", new { returnUrl });
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email).ConfigureAwait(false);
                if (user != null)
                {
                    var result = await _userManager.AddLoginAsync(
                        user: user,
                        login: info).ConfigureAwait(false);
                    if (result.Succeeded)
                    {
                        return await UpdateUser(
                            user: user,
                            info: info,
                            returnUrl: returnUrl,
                            updateClaims: true).ConfigureAwait(false);
                    }
                }
                else if (returnUrl != Url.Content("~/"))
                {
                    user = new User { UserName = Input.Email, Email = Input.Email };
                    var result = await _userManager.CreateAsync(user).ConfigureAwait(false);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation($"User with email '{user.Email}' created an account using {info.LoginProvider} provider.");
                        result = await _userManager.AddLoginAsync(
                            user: user,
                            login: info).ConfigureAwait(false);
                        if (result.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(user, "User").ConfigureAwait(false);
                            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user).ConfigureAwait(false);
                            var confirmEmailUrl = HtmlEncoder.Default.Encode($"{origin}/account/confirm-email?" +
                                                                             $"userId={user.Id}&" +
                                                                             $"code={Uri.EscapeDataString(code)}");
                            var htmlMessage = $"Please confirm your account by <a href='{confirmEmailUrl}'>clicking here</a>.";
                            var body = Encoding.UTF8.GetBytes(htmlMessage);
                            var message = new Message(body);
                            message.UserProperties.Add("email", user.Email);
                            message.UserProperties.Add("subject", "Confirm your email");
                            await _senderClient.SendAsync(message).ConfigureAwait(false);
                            await _userManager.AddClaimsAsync(user, new List<Claim>
                            {
                                new Claim(JwtClaimTypes.GivenName, Input.FirstName),
                                new Claim(JwtClaimTypes.FamilyName, Input.LastName),
                                new Claim(ClaimTypes.Role, "User")
                            }).ConfigureAwait(false);
                            return RedirectToPage("./VerifyEmail", new { user.Email, returnUrl });
                        }
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            LoginProvider = info.LoginProvider;
            Origin = origin;
            ReturnUrl = returnUrl;
            return Page();
        }

        private async Task<IActionResult> UpdateUser(User user, UserLoginInfo info, string returnUrl, bool updateClaims = false)
        {
            if (updateClaims)
            {
                await UpdateClaims(user).ConfigureAwait(false);
            }
            if (await _userManager.IsLockedOutAsync(user).ConfigureAwait(false))
            {
                _logger.LogWarning($"User account with email '{user.Email}' locked out.");
                return RedirectToPage("./Lockout");
            }

            if (!await _userManager.IsEmailConfirmedAsync(user).ConfigureAwait(false))
            {
                return RedirectToPage("./VerifyEmail", new { user.Email, returnUrl });
            }

            if (returnUrl == Url.Content("~/") && !await _userManager.IsInRoleAsync(user, "Admin").ConfigureAwait(false))
            {
                return RedirectToPage("./Login");
            }

            _logger.LogInformation($"User account with email '{user.Email}' logged in with {info.LoginProvider} provider.");
            await _signInManager.SignInAsync(
                user: user,
                isPersistent: false).ConfigureAwait(false);
            return LocalRedirect(returnUrl);
        }

        private async Task UpdateClaims(User user)
        {
            foreach (var claim in await _userManager.GetClaimsAsync(user).ConfigureAwait(false))
            {
                switch (claim.Type)
                {
                    case JwtClaimTypes.GivenName:
                    {
                        await _userManager.ReplaceClaimAsync(
                            user: user,
                            claim: claim,
                            newClaim: new Claim(JwtClaimTypes.GivenName, Input.FirstName)).ConfigureAwait(false);
                        break;
                    }
                    case JwtClaimTypes.FamilyName:
                    {
                        await _userManager.ReplaceClaimAsync(
                            user: user,
                            claim: claim,
                            newClaim: new Claim(JwtClaimTypes.FamilyName, Input.LastName)).ConfigureAwait(false);
                        break;
                    }
                }
            }
        }
    }
}
