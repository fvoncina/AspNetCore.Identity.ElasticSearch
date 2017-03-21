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
			services.AddSingleton(typeof(IUserStore<TUser>), typeof(ElasticUserStore<TUser, TRole>));
			services.AddSingleton(typeof(IRoleStore<TUser>), typeof(ElasticRoleStore<TUser, TRole>));
			return services;
		}

	}
}
