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

			Console.WriteLine();
			identityBuilder.AddUserStore<ElasticUserStore<ElasticUser, ElasticRole>>();
			identityBuilder.AddRoleStore<ElasticRole>();
			identityBuilder.AddUserValidator<UserValidator<ElasticUser>>();
			identityBuilder.AddPasswordValidator<PasswordValidator<ElasticUser>>();
			identityBuilder.AddSignInManager<SignInManager<ElasticUser>>();
			identityBuilder.AddUserManager<UserManager<ElasticUser>>();
			identityBuilder.AddClaimsPrincipalFactory<UserClaimsPrincipalFactory<ElasticUser>>();
			identityBuilder.AddErrorDescriber<IdentityErrorDescriber>();
			
			//services.AddSingleton(typeof(ElasticUserStore<TUser, TRole>), typeof(ElasticUserStore<TUser, TRole>));
			//services.AddSingleton(typeof(ElasticRoleStore<TUser, TRole>), typeof(ElasticRoleStore<TUser, TRole>));
			//services.AddSingleton(typeof(IUserStore<TUser>), sp=> {
			//	return (IUserStore<TUser>)sp.GetRequiredService(typeof(ElasticUserStore<TUser, TRole>));
			//});
			//services.AddSingleton(typeof(IRoleStore<TUser>), sp=> {
			//	return (IRoleStore<TUser>)sp.GetRequiredService(typeof(ElasticRoleStore<TUser, TRole>));
			//});
			//services.AddSingleton(typeof(IRoleClaimStore<TUser>), sp => {
			//	return (IRoleClaimStore<TUser>)sp.GetRequiredService(typeof(ElasticRoleStore<TUser, TRole>));
			//});

			//return new IdentityBuilder(typeof(TUser), services);
			return identityBuilder;
		}

	}
}
