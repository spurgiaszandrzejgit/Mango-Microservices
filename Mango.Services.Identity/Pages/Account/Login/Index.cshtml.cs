using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using Mango.Services.Identity.DbContext;
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

        // the user clicked the "cancel" button
        if (Input.Button != "login")
        {
            if (context != null)
            {
                // This "can't happen", because if the ReturnUrl was null, then the context would be null
                ArgumentNullException.ThrowIfNull(Input.ReturnUrl, nameof(Input.ReturnUrl));

                // if the user cancels, send a result back into IdentityServer as if they 
                // denied the consent (even if this client does not require consent).
                // this will send back an access denied OIDC error response to the client.
                await interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied);

                // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                if (context.IsNativeClient())
                {
                    // The client is native, so this change in how to
                    // return the response is for better UX for the end user.
                    return this.LoadingPage(Input.ReturnUrl);
                }

                return Redirect(Input.ReturnUrl ?? "~/");
            }
            else
            {
                // since we don't have a valid context, then we just go back to the home page
                return Redirect("~/");
            }
        }

        if (ModelState.IsValid)
        {
            var user = await userManager.FindByNameAsync(Input.Username)
           ?? await userManager.FindByEmailAsync(Input.Username);

            if (user != null)
            {
                var result = await signInManager.CheckPasswordSignInAsync(user, Input.Password, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    await events.RaiseAsync(new UserLoginSuccessEvent(
                        user.UserName ?? Input.Username,
                        user.Id,
                        user.UserName ?? Input.Username,
                        clientId: context?.Client.ClientId));

                    Telemetry.Metrics.UserLogin(context?.Client.ClientId, IdentityServerConstants.LocalIdentityProvider);

                    var props = new AuthenticationProperties();
                    if (LoginOptions.AllowRememberLogin && Input.RememberLogin)
                    {
                        props.IsPersistent = true;
                        props.ExpiresUtc = DateTimeOffset.UtcNow.Add(LoginOptions.RememberMeLoginDuration);
                    }

                    // ✅ IMPORTANT: sign in via ASP.NET Identity cookie
                    await signInManager.SignInAsync(user, props);

                    if (context != null)
                    {
                        ArgumentNullException.ThrowIfNull(Input.ReturnUrl, nameof(Input.ReturnUrl));

                        if (context.IsNativeClient())
                            return this.LoadingPage(Input.ReturnUrl);

                        return Redirect(Input.ReturnUrl ?? "~/");
                    }

                    if (Url.IsLocalUrl(Input.ReturnUrl))
                        return Redirect(Input.ReturnUrl);

                    if (string.IsNullOrEmpty(Input.ReturnUrl))
                        return Redirect("~/");

                    throw new ArgumentException("invalid return URL");
                }
            }

            const string error = "invalid credentials";
            await events.RaiseAsync(new UserLoginFailureEvent(Input.Username, error, clientId: context?.Client.ClientId));
            Telemetry.Metrics.UserLoginFailure(context?.Client.ClientId, IdentityServerConstants.LocalIdentityProvider, error);
            ModelState.AddModelError(string.Empty, LoginOptions.InvalidCredentialsErrorMessage);
        }

        // something went wrong, show form with error
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
