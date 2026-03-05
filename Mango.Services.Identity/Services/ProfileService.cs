using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using Mango.Services.Identity.Models;
using System.Security.Claims;

public class ProfileService : IProfileService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfileService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var user = await _userManager.GetUserAsync(context.Subject);
        if (user == null) return;

        var claims = new List<Claim>();
        {
            new Claim("name", user.UserName ?? "");
            new Claim("email", user.Email ?? "");
        };

        // roles -> "role"
        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(r => new Claim("role", r)));

        // optional custom
        if (!string.IsNullOrEmpty(user.FirstName))
            claims.Add(new Claim("given_name", user.FirstName));
        if (!string.IsNullOrEmpty(user.LastName))
            claims.Add(new Claim("family_name", user.LastName));

        // IMPORTANT: respect RequestedClaimTypes
        context.IssuedClaims.AddRange(claims.Where(c => context.RequestedClaimTypes.Contains(c.Type) || c.Type == "role"));
    }

    public async Task IsActiveAsync(IsActiveContext context)
    {
        var user = await _userManager.GetUserAsync(context.Subject);
        context.IsActive = user != null;
    }
}
