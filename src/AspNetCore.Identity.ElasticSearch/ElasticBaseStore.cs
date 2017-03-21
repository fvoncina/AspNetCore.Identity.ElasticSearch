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

namespace AspNetCore.Identity.ElasticSearch
{
	public abstract class ElasticBaseStore<TUser, TRole>
		where TUser : ElasticUser
		where TRole : ElasticRole
	{

		internal readonly string _index;
		internal readonly IElasticClient _nestClient;
		internal readonly int _defaultQuerySize;
		internal readonly int _defaultShards;
		internal readonly int _defaultReplicas;

		#region ctor

		public ElasticBaseStore(IElasticClient nestClient, string index, int defaultQuerySize = 1000, int defaultShards = 1, int defaultReplicas = 0)
		{

			if (nestClient == null)
			{
				throw new ArgumentException(nameof(nestClient));
			}

			if (string.IsNullOrEmpty(index))
			{
				throw new ArgumentException(nameof(index));
			}

			if (defaultShards < 1)
			{
				throw new ArgumentException("defaultShards must be at least 1", nameof(defaultShards));
			}

			_index = index;

			_nestClient = nestClient;

			_defaultQuerySize = defaultQuerySize;

			_defaultShards = defaultShards;

			_defaultReplicas = defaultReplicas;

			EnsureInitialization();

		}

		#endregion

		#region Private Helpers
		internal void EnsureInitialization()
		{

			var createDescriptor = new CreateIndexDescriptor(_index)
				.Settings(s => s
					.NumberOfShards(_defaultShards)
					.NumberOfReplicas(_defaultReplicas)
				).Mappings(m => m
					.Map<TUser>(mm => mm
						.AutoMap()
					)
					.Map<TRole>(mm => mm
						.AutoMap()
					)
					.Map<ElasticUserRole>(mm => mm
						.AutoMap()
					)
				);

			var createIndexResult = _nestClient.CreateIndex(createDescriptor);

			if (!createIndexResult.IsValid)
			{
				throw new InvalidOperationException($"Error creating index {_index}. {createIndexResult.OriginalException.Message}", createIndexResult.OriginalException);

			}

		}

		internal string GetMethodName([CallerMemberName] string callerMemberName = null)
		{
			return callerMemberName;
		}
		#endregion

	}
}
