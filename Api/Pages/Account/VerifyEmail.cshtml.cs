namespace Authentication.Pages.Account
{
    using System;
    using System.Text;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Core.Entities;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Core;
    using Microsoft.Extensions.Logging;

    [AllowAnonymous]
    public class VerifyEmailModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ISenderClient _senderClient;
        private readonly ILogger<ExternalLoginModel> _logger;

        public VerifyEmailModel(
            UserManager<User> userManager,
            ISenderClient senderClient,
            ILogger<ExternalLoginModel> logger)
        {
            _userManager = userManager;
            _senderClient = senderClient;
            _logger = logger;
        }

        public string Email { get; set; }

        public string ReturnUrl { get; set; }

        public string SuccessMessage { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        [TempData]
        [ViewData]
        public string Origin { get; set; }

        public void OnGetAsync(string email, string returnUrl = null)
        {
            Email = email;
            ReturnUrl = returnUrl ?? Url.Content("~/");
        }

        public async Task<IActionResult> OnPostAsync(string email, string returnUrl = null, string origin = null)
        {
            returnUrl ??= Url.Content("~/");
            TempData[nameof(Origin)] = origin;

            var user = await _userManager.FindByEmailAsync(email).ConfigureAwait(false);
            if (user == null)
            {
                ErrorMessage = $"Unable to load user with email '{email}'.";
                return RedirectToPage("./Login", new { ReturnUrl });
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user).ConfigureAwait(false);
            var confirmEmailUrl = HtmlEncoder.Default.Encode($"{origin}/account/confirm-email?" +
                                                             $"userId={user.Id}&" +
                                                             $"code={Uri.EscapeDataString(code)}");
            var htmlMessage = $"Please confirm your account by <a href='{confirmEmailUrl}'>clicking here</a>.";
            var body = Encoding.UTF8.GetBytes(htmlMessage);
            var message = new Message(body);
            message.UserProperties.Add("email", email);
            message.UserProperties.Add("subject", "Confirm your email");
            await _senderClient.SendAsync(message).ConfigureAwait(false);
            _logger.LogInformation($"Email verification email sent to '{user.Email}'.");

            SuccessMessage = "Verification email sent. Please check your email.";
            Origin = origin;
            ReturnUrl = returnUrl;
            return Page();
        }
    }
}
