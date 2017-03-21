using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCore.Identity.ElasticSearch
{
	public class ElasticSearchBuilder<TUser, TRole>
		where TUser : ElasticUser
		where TRole : ElasticRole
	{

		public ElasticSearchBuilder(IServiceCollection services)
		{
			UserType = typeof(TUser);
			RoleType = typeof(TRole);
			Services = services;
		}

		public IServiceCollection Services { get; set; }

		public Type UserType { get; set; }

		public Type RoleType { get; set; }

		private ElasticSearchBuilder<TUser, TRole> AddScoped(Type serviceType, Type concreteType)
		{
			Services.AddScoped(serviceType, concreteType);
			return this;
		}

		private ElasticSearchBuilder<TUser, TRole> AddSingleton(Type serviceType, Type concreteType)
		{
			Services.AddSingleton(serviceType, concreteType);
			return this;
		}

		public ElasticSearchBuilder<TUser, TRole> AddUserStore<T>() where T : class
		{
			return AddSingleton(typeof(IUserStore<>).MakeGenericType(UserType), typeof(T));
		}

		public ElasticSearchBuilder<TUser, TRole> AddUserStore()
		{
			return AddUserStore<ElasticSearchBuilder<TUser, TRole>>();
		}

		public ElasticSearchBuilder<TUser, TRole> AddRoleStore<T>() where T : class
		{
			return AddSingleton(typeof(IRoleStore<>).MakeGenericType(RoleType), typeof(T));
		}

		public ElasticSearchBuilder<TUser, TRole> AddRoleStore()
		{
			return AddRoleStore<ElasticRoleStore<TUser, TRole>>();
		}

		public ElasticSearchBuilder<TUser, TRole> AddRoleUsersStore<T>() where T : class
		{
			return AddSingleton(typeof(T), typeof(T));
		}

		public ElasticSearchBuilder<TUser, TRole> AddRoleUsersStore()
		{
			return AddRoleUsersStore<ElasticUserStore<TUser, TRole>>();
		}

	}
}
