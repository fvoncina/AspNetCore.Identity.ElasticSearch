using Microsoft.AspNetCore.Identity;
using Nest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;
using System.Linq;
using Microsoft.Extensions.Options;

namespace AspNetCore.Identity.ElasticSearch
{
	public abstract class ElasticBaseStore<TUser, TRole>
		where TUser : ElasticUser
		where TRole : ElasticRole
	{

		#region ctor

		public ElasticBaseStore(IElasticClient nestClient, IOptions<ElasticOptions> options) : this(nestClient, options.Value)
		{

		}

		public ElasticBaseStore(IElasticClient nestClient, ElasticOptions options)
		{

			if (nestClient == null)
			{
				throw new ArgumentException(nameof(nestClient));
			}

			if (options == null)
			{
				throw new ArgumentException(nameof(options));
			}

			if (string.IsNullOrEmpty(options.Index))
			{
				throw new ArgumentException(nameof(options.Index));
			}

			if (string.IsNullOrEmpty(options.UsersType))
			{
				throw new ArgumentException(nameof(options.UsersType));
			}

			if (string.IsNullOrEmpty(options.RolesType))
			{
				throw new ArgumentException(nameof(options.RolesType));
			}

			if (string.IsNullOrEmpty(options.UserRolesType))
			{
				throw new ArgumentException(nameof(options.UserRolesType));
			}

			NestClient = nestClient;

			Options = options;

			EnsureInitialization();

		}

		#endregion

		#region props
		public ElasticOptions Options { get; set; }

		public IElasticClient NestClient { get; set; }
		#endregion

		#region Private Helpers
		internal void EnsureInitialization()
		{

			if (!NestClient.IndexExists(Options.Index).Exists)
			{

				var createDescriptor = new CreateIndexDescriptor(Options.Index)
					.Settings(s => s
						.NumberOfShards(Options.DefaultShards)
						.NumberOfReplicas(Options.DefaultReplicas)
					).Mappings(m => m
						.Map<TUser>(Options.UsersType, mm => mm
								.AutoMap()
						)
						.Map<TRole>(Options.RolesType, mm => mm
							.AutoMap()
						)
						.Map<ElasticUserRole>(Options.UserRolesType, mm => mm
							.AutoMap()
						)
				);

				var createIndexResult = NestClient.CreateIndex(createDescriptor);

				if (!createIndexResult.IsValid)
				{
					throw new InvalidOperationException($"Error creating index {Options.Index}. {createIndexResult.ServerError}");

				}

			}

		}

		internal string GetMethodName([CallerMemberName] string callerMemberName = null)
		{
			return callerMemberName;
		}
		#endregion

	}
}
