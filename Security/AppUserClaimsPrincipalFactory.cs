using System.Security.Claims;
using bookTracker.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace bookTracker.Security;

public class AppUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<AppUser, IdentityRole>
{
    public AppUserClaimsPrincipalFactory(
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
    { }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        var first = (user.FirstName ?? "").Trim();
        var last = (user.LastName ?? "").Trim();
        var full = $"{first} {last}".Trim();

        if (!string.IsNullOrWhiteSpace(first))
            identity.AddClaim(new Claim("FirstName", first));

        if (!string.IsNullOrWhiteSpace(last))
            identity.AddClaim(new Claim("LastName", last));

        if (!string.IsNullOrWhiteSpace(full))
            identity.AddClaim(new Claim("FullName", full));

        return identity;
    }
}
