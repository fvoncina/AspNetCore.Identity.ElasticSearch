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
		public static IServiceCollection AddElasticSearchIdentity<TUser, TRole>(this IServiceCollection services)
			where TUser : ElasticUser
			where TRole : ElasticRole
		{
			services.AddSingleton(typeof(ElasticUserStore<TUser, TRole>), typeof(ElasticUserStore<TUser, TRole>));
			services.AddSingleton(typeof(ElasticRoleStore<TUser, TRole>), typeof(ElasticRoleStore<TUser, TRole>));
			services.AddSingleton(typeof(IUserStore<TUser>), sp=> {
				return (IUserStore<TUser>)sp.GetRequiredService(typeof(ElasticUserStore<TUser, TRole>));
			});
			services.AddSingleton(typeof(IRoleStore<TUser>), sp=> {
				return (IRoleStore<TUser>)sp.GetRequiredService(typeof(ElasticRoleStore<TUser, TRole>));
			});
			services.AddSingleton(typeof(IRoleClaimStore<TUser>), sp => {
				return (IRoleClaimStore<TUser>)sp.GetRequiredService(typeof(ElasticRoleStore<TUser, TRole>));
			});
			return services;
		}

	}
}
