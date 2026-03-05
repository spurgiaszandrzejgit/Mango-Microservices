using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using Mango.Services.Identity.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Mango.Services.Identity.Pages.Account.Login;

[SecurityHeaders]
[AllowAnonymous]
public class Index(
    IIdentityServerInteractionService interaction,
    IAuthenticationSchemeProvider schemeProvider,
    IIdentityProviderStore identityProviderStore,
    IEventService events,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager)
    : PageModel
{
    public ViewModel View { get; set; } = default!;

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    // this is where you would plug in your own custom identity management library (e.g. ASP.NET Identity)

    public async Task<IActionResult> OnGet(string? returnUrl)
    {
        await BuildModelAsync(returnUrl);

        if (View.IsExternalLoginOnly)
        {
            // we only have one option for logging in and it's an external provider
            return RedirectToPage("/ExternalLogin/Challenge", new { scheme = View.ExternalLoginScheme, returnUrl });
        }

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        // check if we are in the context of an authorization request
        var context = await interaction.GetAuthorizationContextAsync(Input.ReturnUrl);

        // user clicked "cancel"
        if (Input.Button != "login")
        {
            if (context != null)
            {
                ArgumentNullException.ThrowIfNull(Input.ReturnUrl, nameof(Input.ReturnUrl));

                await interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied);

                if (context.IsNativeClient())
                    return this.LoadingPage(Input.ReturnUrl);

                return Redirect(Input.ReturnUrl ?? "~/");
            }

            return Redirect("~/");
        }

        if (!ModelState.IsValid)
        {
            await BuildModelAsync(Input.ReturnUrl);
            return Page();
        }

        // find user by username or email
        var user = await userManager.FindByNameAsync(Input.Username)
                   ?? await userManager.FindByEmailAsync(Input.Username);

        if (user == null)
        {
            const string error = "invalid credentials";
            await events.RaiseAsync(new UserLoginFailureEvent(Input.Username, error, clientId: context?.Client.ClientId));
            Telemetry.Metrics.UserLoginFailure(context?.Client.ClientId, IdentityServerConstants.LocalIdentityProvider, error);
            ModelState.AddModelError(string.Empty, LoginOptions.InvalidCredentialsErrorMessage);

            await BuildModelAsync(Input.ReturnUrl);
            return Page();
        }

        // sign-in (this issues the auth cookie)
        var result = await signInManager.PasswordSignInAsync(
            user,
            Input.Password,
            Input.RememberLogin && LoginOptions.AllowRememberLogin,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            await events.RaiseAsync(new UserLoginSuccessEvent(
                user.UserName ?? Input.Username,
                user.Id,
                user.UserName ?? Input.Username,
                clientId: context?.Client.ClientId));

            Telemetry.Metrics.UserLogin(context?.Client.ClientId, IdentityServerConstants.LocalIdentityProvider);

            // redirect back to OIDC client if this was an authorization request
            if (context != null)
            {
                ArgumentNullException.ThrowIfNull(Input.ReturnUrl, nameof(Input.ReturnUrl));

                if (context.IsNativeClient())
                    return this.LoadingPage(Input.ReturnUrl);

                return Redirect(Input.ReturnUrl ?? "~/");
            }

            // local page redirect
            if (Url.IsLocalUrl(Input.ReturnUrl))
                return Redirect(Input.ReturnUrl);

            if (string.IsNullOrEmpty(Input.ReturnUrl))
                return Redirect("~/");

            throw new ArgumentException("invalid return URL");
        }

        if (result.RequiresTwoFactor)
        {
            // if later you add 2FA, redirect to 2FA page here
            ModelState.AddModelError(string.Empty, "Two-factor authentication is required.");
        }
        else if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Account is locked.");
        }
        else if (result.IsNotAllowed)
        {
            // e.g. Email not confirmed if you enforce it
            ModelState.AddModelError(string.Empty, "Account is not allowed to sign in.");
        }
        else
        {
            const string error = "invalid credentials";
            await events.RaiseAsync(new UserLoginFailureEvent(Input.Username, error, clientId: context?.Client.ClientId));
            Telemetry.Metrics.UserLoginFailure(context?.Client.ClientId, IdentityServerConstants.LocalIdentityProvider, error);
            ModelState.AddModelError(string.Empty, LoginOptions.InvalidCredentialsErrorMessage);
        }

        await BuildModelAsync(Input.ReturnUrl);
        return Page();
    }

    private async Task BuildModelAsync(string? returnUrl)
    {
        Input = new InputModel
        {
            ReturnUrl = returnUrl
        };

        var context = await interaction.GetAuthorizationContextAsync(returnUrl);
        if (context?.IdP != null)
        {
            var scheme = await schemeProvider.GetSchemeAsync(context.IdP);
            if (scheme != null)
            {
                var local = context.IdP == Duende.IdentityServer.IdentityServerConstants.LocalIdentityProvider;

                // this is meant to short circuit the UI and only trigger the one external IdP
                View = new ViewModel
                {
                    EnableLocalLogin = local,
                };

                Input.Username = context.LoginHint;

                if (!local)
                {
                    View.ExternalProviders = [new ViewModel.ExternalProvider(authenticationScheme: context.IdP, displayName: scheme.DisplayName)];
                }
            }

            return;
        }

        var schemes = await schemeProvider.GetAllSchemesAsync();

        var providers = schemes
            .Where(x => x.DisplayName != null)
            .Select(x => new ViewModel.ExternalProvider
            (
                authenticationScheme: x.Name,
                displayName: x.DisplayName ?? x.Name
            )).ToList();

        var dynamicSchemes = (await identityProviderStore.GetAllSchemeNamesAsync())
            .Where(x => x.Enabled)
            .Select(x => new ViewModel.ExternalProvider
            (
                authenticationScheme: x.Scheme,
                displayName: x.DisplayName ?? x.Scheme
            ));
        providers.AddRange(dynamicSchemes);


        var allowLocal = true;
        var client = context?.Client;
        if (client != null)
        {
            allowLocal = client.EnableLocalLogin;
            if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Count != 0)
            {
                providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
            }
        }

        View = new ViewModel
        {
            AllowRememberLogin = LoginOptions.AllowRememberLogin,
            EnableLocalLogin = allowLocal && LoginOptions.AllowLocalLogin,
            ExternalProviders = providers.ToArray()
        };
    }
}
