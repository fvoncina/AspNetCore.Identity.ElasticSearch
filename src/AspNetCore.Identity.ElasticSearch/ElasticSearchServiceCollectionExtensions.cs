using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace AspNetCore.Identity.ElasticSearch
{
	public static class ElasticSearchServiceCollectionExtensions
	{
		public static IdentityBuilder AddElasticsearchIdentity(this IdentityBuilder identityBuilder)
		{
			identityBuilder.AddUserStore<ElasticUserStore<ElasticUser, ElasticRole>>();
			identityBuilder.AddRoleStore<ElasticRole>();
			identityBuilder.AddUserValidator<UserValidator<ElasticUser>>();
			identityBuilder.AddPasswordValidator<PasswordValidator<ElasticUser>>();
			identityBuilder.AddSignInManager<SignInManager<ElasticUser>>();
			identityBuilder.AddUserManager<UserManager<ElasticUser>>();
			identityBuilder.AddClaimsPrincipalFactory<UserClaimsPrincipalFactory<ElasticUser>>();
			identityBuilder.AddErrorDescriber<IdentityErrorDescriber>();
			
			return identityBuilder;
		}

	}
}
