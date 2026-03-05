using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mango.Services.Identity.DbContext;
using Mango.Services.Identity.Models;
using Microsoft.AspNetCore.Identity;

namespace Mango.Services.Identity.Pages.Account.Create;

[SecurityHeaders]
[AllowAnonymous]
public class Index(
    IIdentityServerInteractionService interaction,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager)
    : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = default!;

    // this is where you would plug in your own custom identity management library (e.g. ASP.NET Identity)

    public IActionResult OnGet(string? returnUrl)
    {
        Input = new InputModel { ReturnUrl = returnUrl };
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        var context = await interaction.GetAuthorizationContextAsync(Input.ReturnUrl);

        if (Input.Button != "create")
        {
            if (context != null)
            {
                await interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied);

                if (context.IsNativeClient())
                    return this.LoadingPage(Input.ReturnUrl);

                return Redirect(Input.ReturnUrl ?? "~/");
            }

            return Redirect("~/");
        }

        if (!ModelState.IsValid)
            return Page();

        var existing = await userManager.FindByNameAsync(Input.Username)
                      ?? await userManager.FindByEmailAsync(Input.Email);

        if (existing != null)
        {
            ModelState.AddModelError(string.Empty, "User already exists.");
            return Page();
        }

        var user = new ApplicationUser
        {
            UserName = Input.Username,
            Email = Input.Email,
            EmailConfirmed = true,
            FirstName = Input.Name,
            LastName = ""
        };

        var createResult = await userManager.CreateAsync(user, Input.Password);
        if (!createResult.Succeeded)
        {
            foreach (var err in createResult.Errors)
                ModelState.AddModelError(string.Empty, err.Description);

            return Page();
        }

        await userManager.AddToRoleAsync(user, "Customer");

        await signInManager.SignInAsync(user, isPersistent: false);

        if (context != null)
        {
            if (context.IsNativeClient())
                return this.LoadingPage(Input.ReturnUrl);

            return Redirect(Input.ReturnUrl ?? "~/");
        }

        if (Url.IsLocalUrl(Input.ReturnUrl))
            return Redirect(Input.ReturnUrl!);

        return Redirect("~/");
    }
}
