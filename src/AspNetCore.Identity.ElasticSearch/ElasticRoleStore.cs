using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Identity.ElasticSearch
{
	public class ElasticRoleStore<TUser, TRole> :
		ElasticBaseStore<TUser, TRole>,
		IRoleStore<TRole>,
		IRoleClaimStore<TRole>
		where TUser : ElasticUser
		where TRole : ElasticRole
	{

		#region ctor

		public ElasticRoleStore(IElasticClient nestClient, IOptions<ElasticOptions> options) : base(nestClient, options)
		{

		}

		public ElasticRoleStore(IElasticClient nestClient, ElasticOptions options):base(nestClient, options)
		{			

		}

		#endregion

		#region IRoleClaimStore
		public Task<IList<Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (role == null)
			{
				throw new ArgumentNullException(nameof(role));
			}

			var claims = role.Claims.ToClaims();

			return Task.FromResult(claims);
		}

		public Task AddClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (role == null)
			{
				throw new ArgumentNullException(nameof(role));
			}

			if (claim == null)
			{
				throw new ArgumentNullException(nameof(claim));
			}

			role.Claims.AddClaim(claim);

			return Task.FromResult(0);
		}

		public Task RemoveClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (role == null)
			{
				throw new ArgumentNullException(nameof(role));
			}

			if (claim == null)
			{
				throw new ArgumentNullException(nameof(claim));
			}

			role.Claims.RemoveClaim(claim);

			return Task.FromResult(0);
		}

		public async Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken)
		{
			if (role == null)
			{
				throw new ArgumentNullException(nameof(role));
			}

			cancellationToken.ThrowIfCancellationRequested();

			var esResult = await NestClient.IndexAsync<TRole>(role, r => r
				.Index(Options.Index)
				.Type(Options.RolesType)
				.Refresh(Elasticsearch.Net.Refresh.True)
				, cancellationToken
			);

			if (!esResult.IsValid)
			{
				throw new Exception($"ElasticSearch Error in {GetMethodName()}", esResult.OriginalException);
			}

			return IdentityResult.Success;

		}

		public async Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken)
		{
			if (role == null)
			{
				throw new ArgumentNullException(nameof(role));
			}

			cancellationToken.ThrowIfCancellationRequested();

			var esResult = await NestClient.IndexAsync<TRole>(role, r => r
				.Index(Options.Index)
				.Type(Options.RolesType)
				.Refresh(Elasticsearch.Net.Refresh.True)
				, cancellationToken
			);

			if (!esResult.IsValid)
			{
				throw new Exception($"ElasticSearch Error in {GetMethodName()}", esResult.OriginalException);
			}

			return IdentityResult.Success;
		}

		public async Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken)
		{
			if (role == null)
			{
				throw new ArgumentNullException(nameof(role));
			}

			cancellationToken.ThrowIfCancellationRequested();

			var esResult = await NestClient.DeleteAsync<TRole>(role, r => r
				.Index(Options.Index)
				.Type(Options.RolesType)
				.Refresh(Elasticsearch.Net.Refresh.True)
				, cancellationToken
			);

			if (!esResult.IsValid)
			{
				throw new Exception($"ElasticSearch Error in {GetMethodName()}", esResult.OriginalException);
			}

			return IdentityResult.Success;
		}

		public Task<string> GetRoleIdAsync(TRole role, CancellationToken cancellationToken)
		{
			if (role == null)
			{
				throw new ArgumentNullException(nameof(role));
			}

			return Task.FromResult(role.Id);
		}

		public Task<string> GetRoleNameAsync(TRole role, CancellationToken cancellationToken)
		{
			if (role == null)
			{
				throw new ArgumentNullException(nameof(role));
			}

			return Task.FromResult(role.Name);
		}

		public Task SetRoleNameAsync(TRole role, string roleName, CancellationToken cancellationToken)
		{
			throw new NotSupportedException("Changing the role name is not supported.");
		}

		public Task<string> GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken)
		{
			if (role == null)
			{
				throw new ArgumentNullException(nameof(role));
			}

			return Task.FromResult(role.Normalized);
		}

		public Task SetNormalizedRoleNameAsync(TRole role, string normalizedName, CancellationToken cancellationToken)
		{
			throw new NotSupportedException("Changing the role normalized name is not supported.");
		}

		public async Task<TRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
		{
			if (string.IsNullOrEmpty(roleId))
			{
				throw new ArgumentException(nameof(roleId));
			}

			cancellationToken.ThrowIfCancellationRequested();

			var esResult = await NestClient.GetAsync<TRole>(roleId, r => r
				.Index(Options.Index)
				.Type(Options.RolesType)
				, cancellationToken
			);

			if (!esResult.IsValid)
			{
				throw new Exception($"ElasticSearch Error in {GetMethodName()}", esResult.OriginalException);
			}

			return esResult.Source;
		}

		public async Task<TRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
		{
			if (string.IsNullOrEmpty(normalizedRoleName))
			{
				throw new ArgumentException(nameof(normalizedRoleName));
			}

			cancellationToken.ThrowIfCancellationRequested();

			var esResult = await NestClient.SearchAsync<TRole>(r => r
				.Index(Options.Index)
				.Type(Options.RolesType)
				.Size(1)
				.Query(q => q
					.Term(t => t.Normalized, normalizedRoleName.GenerateSlug())
				), cancellationToken
			);

			if (!esResult.IsValid)
			{
				throw new Exception($"ElasticSearch Error in {GetMethodName()}", esResult.OriginalException);
			}

			return esResult.Documents.FirstOrDefault();
		}

		public void Dispose()
		{
		}
		#endregion

	}
}
