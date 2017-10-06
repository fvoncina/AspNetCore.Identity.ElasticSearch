using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace AspNetCore.Identity.ElasticSearch
{
	public class UserClaimsPrincipalFactory<TUser> : IUserClaimsPrincipalFactory<TUser>
		where TUser : class
	{
		public UserClaimsPrincipalFactory(
			UserManager<TUser> userManager,
			IOptions<IdentityOptions> optionsAccessor)
		{
			if (optionsAccessor?.Value == null)
			{
				throw new ArgumentNullException(nameof(optionsAccessor));
			}

			UserManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
			Options = optionsAccessor.Value;
		}

		public UserManager<TUser> UserManager { get; }

		public IdentityOptions Options { get; }

		public virtual async Task<ClaimsPrincipal> CreateAsync(TUser user)
		{
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}

			var userId = await UserManager.GetUserIdAsync(user);
			var userName = await UserManager.GetUserNameAsync(user);

			//var id = new ClaimsIdentity(Options.Cookies.ApplicationCookieAuthenticationScheme,
			//	Options.ClaimsIdentity.UserNameClaimType,
			//	Options.ClaimsIdentity.RoleClaimType);

			var id = new ClaimsIdentity(IdentityConstants.ApplicationScheme);

			id.AddClaim(new Claim(Options.ClaimsIdentity.UserIdClaimType, userId));
			id.AddClaim(new Claim(Options.ClaimsIdentity.UserNameClaimType, userName));
			if (UserManager.SupportsUserSecurityStamp)
			{
				id.AddClaim(new Claim(Options.ClaimsIdentity.SecurityStampClaimType,
					await UserManager.GetSecurityStampAsync(user)));
			}
			if (UserManager.SupportsUserRole)
			{
				var roles = await UserManager.GetRolesAsync(user);
				foreach (var roleName in roles)
				{
					id.AddClaim(new Claim(Options.ClaimsIdentity.RoleClaimType, roleName));
				}
			}
			if (UserManager.SupportsUserClaim)
			{
				id.AddClaims(await UserManager.GetClaimsAsync(user));
			}

			return new ClaimsPrincipal(id);
		}
	}
}
