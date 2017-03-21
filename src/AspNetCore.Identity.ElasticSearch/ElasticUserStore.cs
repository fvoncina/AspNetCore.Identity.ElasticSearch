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
	public class ElasticUserStore<TUser, TRole> :
		ElasticBaseStore<TUser, TRole>,
		IUserStore<TUser>,
		IUserLoginStore<TUser>,
		IUserClaimStore<TUser>,
		IUserRoleStore<TUser>,
		IUserPasswordStore<TUser>,
		IUserSecurityStampStore<TUser>,
		IUserTwoFactorStore<TUser>,
		IUserEmailStore<TUser>,
		IUserPhoneNumberStore<TUser>
		where TUser : ElasticUser
		where TRole : ElasticRole

	{

		#region ctor

		public ElasticUserStore(IElasticClient nestClient, string index, int defaultQuerySize = 1000, int defaultShards = 1, int defaultReplicas = 0)
			: base(nestClient, index, defaultQuerySize, defaultShards, defaultReplicas)
		{

		}

		#endregion

		#region IUserStore
		public void Dispose()
		{
		}

		public async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken)
		{

			if (user == null)
			{
				throw new ArgumentException(nameof(user));
			}

			cancellationToken.ThrowIfCancellationRequested();

			if (string.IsNullOrEmpty(user.Id))
			{
				user.Id = Guid.NewGuid().ToString();
			}

			var esResult = await _nestClient.IndexAsync<TUser>(user, r => r
				.Index(_index)
				.Refresh(Elasticsearch.Net.Refresh.True)
				, cancellationToken
			);

			if (!esResult.IsValid)
			{
				throw new Exception($"ElasticSearch Error in {GetMethodName()}", esResult.OriginalException);
			}

			return IdentityResult.Success;

		}

		public async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken)
		{

			if (user == null)
			{
				throw new ArgumentException(nameof(user));
			}

			cancellationToken.ThrowIfCancellationRequested();

			var esResult = await _nestClient.DeleteAsync<TUser>(user, r => r
				.Index(_index)
				.Refresh(Elasticsearch.Net.Refresh.True)
				, cancellationToken
			);

			if (!esResult.IsValid)
			{
				throw new Exception($"ElasticSearch Error in {GetMethodName()}", esResult.OriginalException);
			}

			return IdentityResult.Success;

		}

		public async Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
		{

			if (string.IsNullOrEmpty(userId))
			{
				throw new ArgumentException(nameof(userId));
			}

			cancellationToken.ThrowIfCancellationRequested();

			var esResult = await _nestClient.GetAsync<TUser>(userId, r => r.Index(_index), cancellationToken);

			if (!esResult.IsValid)
			{
				throw new Exception($"ElasticSearch Error in {GetMethodName()}", esResult.OriginalException);
			}

			return esResult.Source;

		}

		public async Task<TUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
		{

			if (string.IsNullOrEmpty(loginProvider))
			{
				throw new ArgumentException(nameof(loginProvider));
			}

			if (string.IsNullOrEmpty(providerKey))
			{
				throw new ArgumentException(nameof(providerKey));
			}

			cancellationToken.ThrowIfCancellationRequested();

			var esResult = await _nestClient.SearchAsync<TUser>(r => r
				.Index(_index)
				.Size(1)
				.Query(q => q
					.Nested(n => n
						.Path(p => p.Logins)
						.Query(qq => qq
							.Bool(b => b
								.Must(
									m => m.Term(_nestClient.Infer.NestedProperty<ElasticUser, ElasticUserLogin>(x => x.Logins, x => x.LoginProvider), loginProvider),
									m => m.Term(_nestClient.Infer.NestedProperty<ElasticUser, ElasticUserLogin>(x => x.Logins, x => x.ProviderKey), providerKey)
								)
							)
						)
					)
				), cancellationToken
			);

			if (!esResult.IsValid)
			{
				throw new Exception($"ElasticSearch Error in {GetMethodName()}", esResult.OriginalException);
			}

			return esResult.Documents.FirstOrDefault();

		}

		public async Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
		{

			if (string.IsNullOrEmpty(normalizedUserName))
			{
				throw new ArgumentException(nameof(normalizedUserName));
			}

			cancellationToken.ThrowIfCancellationRequested();

			var esResult = await _nestClient.SearchAsync<TUser>(r => r
				.Index(_index)
				.Size(1)
				.Query(q => q
					.Term(t => t.Normalized, normalizedUserName.GenerateSlug())
				), cancellationToken
			);

			if (!esResult.IsValid)
			{
				throw new Exception($"ElasticSearch Error in {GetMethodName()}", esResult.OriginalException);
			}

			return esResult.Documents.FirstOrDefault();

		}

		public Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken)
		{

			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}

			var rv = (IList<UserLoginInfo>)user.Logins
				.Select(login => new UserLoginInfo(login.LoginProvider, login.ProviderKey, login.ProviderDisplayName))
				.ToList();

			return Task.FromResult(rv);

		}

		public Task<string> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken)
		{

			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}

			return Task.FromResult(user.Normalized);

		}

		public Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken)
		{

			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}

			return Task.FromResult(user.Id);

		}

		public Task<string> GetUserNameAsync(TUser user, CancellationToken cancellationToken)
		{

			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}

			return Task.FromResult(user.UserName);

		}

		public Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
		{

			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}

			if (loginProvider == null)
			{
				throw new ArgumentNullException(nameof(loginProvider));
			}

			if (providerKey == null)
			{
				throw new ArgumentNullException(nameof(providerKey));
			}

			user.Logins.RemoveAll(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey);

			return Task.FromResult(0);

		}

		public Task SetNormalizedUserNameAsync(TUser user, string normalizedName, CancellationToken cancellationToken)
		{
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}

			if (string.IsNullOrEmpty(normalizedName))
			{
				throw new ArgumentNullException(nameof(normalizedName));
			}

			user.Normalized = normalizedName;

			return Task.FromResult(0);
		}

		public Task SetUserNameAsync(TUser user, string userName, CancellationToken cancellationToken)
		{

			if (user == null)
			{
				throw new ArgumentException(nameof(user));
			}

			if (string.IsNullOrEmpty(userName))
			{
				throw new ArgumentException(nameof(userName));
			}

			user.UserName = userName;

			return Task.FromResult(0);

		}

		public async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken)
		{

			if (user == null)
			{
				throw new ArgumentException(nameof(user));
			}

			var esResult = await _nestClient.IndexAsync(user, r => r
				.Index(_index)
				.Refresh(Elasticsearch.Net.Refresh.True)
				, cancellationToken
			);

			if (!esResult.IsValid)
			{
				throw new Exception($"ElasticSearch Error in {GetMethodName()}", esResult.OriginalException);
			}

			return IdentityResult.Success;

		}
		#endregion

		#region IUserLoginStore
		public Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken)
		{
			if (user == null)
			{
				throw new ArgumentException(nameof(user));
			}

			if (login == null)
			{
				throw new ArgumentException(nameof(login));
			}

			if (!user.Logins.Any(x => x.LoginProvider == login.LoginProvider && x.ProviderKey == login.ProviderKey))
			{

				user.Logins.Add(new ElasticUserLogin(login));

			}

			return Task.FromResult(0);
		}
		#endregion

		#region IUserClaimStore
		public Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken)
		{
			if (user == null)
			{
				throw new ArgumentException(nameof(user));
			}

			var rv = (IList<Claim>)user.Claims
				.Select(x => x.AsClaim())
				.ToList();

			return Task.FromResult(rv);
		}

		public Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
		{
			if (user == null)
			{
				throw new ArgumentException(nameof(user));
			}

			if (claims == null)
			{
				throw new ArgumentException(nameof(claims));
			}

			foreach (var newClaim in claims)
			{
				var currentClaim = user.Claims.Find(x => x.Type == newClaim.Type);
				if (currentClaim != null)
				{
					user.Claims.Remove(currentClaim);
				}
				user.Claims.Add(new ElasticClaim(newClaim));
			}

			return Task.FromResult(0);
		}

		public Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
		{
			if (user == null)
			{
				throw new ArgumentException(nameof(user));
			}

			if (claim == null)
			{
				throw new ArgumentException(nameof(claim));
			}

			if (newClaim == null)
			{
				throw new ArgumentException(nameof(newClaim));
			}

			var currentClaim = user.Claims.Find(x => x.Type == claim.Type);

			if (currentClaim != null)
			{
				user.Claims.Remove(currentClaim);
			}

			user.Claims.Add(new ElasticClaim(newClaim));

			return Task.FromResult(0);
		}

		public Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
		{
			if (user == null)
			{
				throw new ArgumentException(nameof(user));
			}

			if (claims == null)
			{
				throw new ArgumentException(nameof(claims));
			}

			foreach (var newClaim in claims)
			{

				var currentClaim = user.Claims.Find(x => x == newClaim);

				if (currentClaim != null)
				{
					user.Claims.Remove(currentClaim);
				}

			}

			return Task.FromResult(0);
		}

		public async Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
		{

			if (claim == null)
			{
				throw new ArgumentException(nameof(claim));
			}

			cancellationToken.ThrowIfCancellationRequested();

			var esResult = await _nestClient.SearchAsync<TUser>(r => r
				.Index(_index)
				.Size(_defaultQuerySize)
				.Query(q => q
					.Nested(n => n
						.Path(p => p.Claims)
						.Query(qq => qq
							.Bool(b => b
								.Must(
									m => m.Term(_nestClient.Infer.NestedProperty<ElasticUser, ElasticClaim>(x => x.Claims, x => x.Type), claim.Type),
									m => m.Term(_nestClient.Infer.NestedProperty<ElasticUser, ElasticClaim>(x => x.Claims, x => x.Value), claim.Value)
								)
							)
						)


					)
				), cancellationToken
			);

			if (!esResult.IsValid)
			{
				throw new Exception($"ElasticSearch Error in {GetMethodName()}", esResult.OriginalException);
			}

			return (IList<TUser>)esResult.Documents.ToList();

		}
		#endregion

		#region IUserRoleStore
		public async Task AddToRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
		{
			if (user == null)
			{
				throw new ArgumentException(nameof(user));
			}

			if (string.IsNullOrEmpty(roleName))
			{
				throw new ArgumentException(nameof(roleName));
			}

			cancellationToken.ThrowIfCancellationRequested();

			if (!await IsInRoleAsync(user, roleName, cancellationToken))
			{
				var esResult = await _nestClient.IndexAsync(new ElasticUserRole(roleName.GenerateSlug(), user.Id), r => r
					.Index(_index)
					.Refresh(Elasticsearch.Net.Refresh.True)
					, cancellationToken
				);
				if (!esResult.IsValid)
				{
					throw new Exception($"ElasticSearch Error in {GetMethodName()}", esResult.OriginalException);
				}
			}
		}

		public async Task RemoveFromRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
		{
			if (user == null)
			{
				throw new ArgumentException(nameof(user));
			}

			if (string.IsNullOrEmpty(roleName))
			{
				throw new ArgumentException(nameof(roleName));
			}

			cancellationToken.ThrowIfCancellationRequested();

			var esResult = await _nestClient.DeleteByQueryAsync<ElasticUserRole>(r => r
				.Index(_index)
				.Size(_defaultQuerySize)
				.Refresh(true)
				.Query(q => q
					.Bool(b => b
						.Must(
							m => m.Term(t => t.NormalizedRoleName, roleName.GenerateSlug()),
							m => m.Term(t => t.UserId, user.Id)
						)
					)
				)
				, cancellationToken
			);
			if (!esResult.IsValid)
			{
				throw new Exception($"ElasticSearch Error in {GetMethodName()}", esResult.OriginalException);
			}

		}

		public async Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken)
		{
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}

			var esResult = await _nestClient.SearchAsync<ElasticUserRole>(r => r
				.Index(_index)
				.Size(_defaultQuerySize)
				.Query(q => q
					.Term(t => t.UserId, user.Id)
				)
				, cancellationToken
			);
			return esResult.Documents.Select(x => x.NormalizedRoleName).Distinct().ToList();
		}

		public async Task<bool> IsInRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
		{
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}

			if (roleName == null)
			{
				throw new ArgumentNullException(nameof(roleName));
			}

			var esResult = await _nestClient.SearchAsync<ElasticUserRole>(r => r
				.Index(_index)
				.Size(1)
				.Query(q => q
					.Bool(b => b
						.Must(
							m => m.Term(t => t.NormalizedRoleName, roleName.GenerateSlug()),
							m => m.Term(t => t.UserId, user.Id)
						)
					)
				)
				, cancellationToken
			);
			return esResult.Total != 0;
		}

		public async Task<IList<TUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
		{
			if (string.IsNullOrEmpty(roleName))
			{
				throw new ArgumentException(nameof(roleName));
			}

			cancellationToken.ThrowIfCancellationRequested();

			var esUserIds = await _nestClient.SearchAsync<ElasticUserRole>(r => r
				.Index(_index)
				.Size(_defaultQuerySize)
				.Query(q => q
					.Term(t => t.NormalizedRoleName, roleName.GenerateSlug())
				), cancellationToken
			);

			var esUsers = await _nestClient.GetManyAsync<TUser>(esUserIds.Documents.Select(x => x.Id), _index, null, cancellationToken);

			return esUsers.Select(x => x.Source).ToList();

		}
		#endregion

		#region IUserPasswordStore
		public Task SetPasswordHashAsync(TUser user, string passwordHash, CancellationToken cancellationToken)
		{

			if (user == null)
			{
				throw new ArgumentException(nameof(user));
			}

			user.PasswordHash = passwordHash;

			return Task.FromResult(0);

		}

		public Task<string> GetPasswordHashAsync(TUser user, CancellationToken cancellationToken)
		{

			if (user == null)
			{
				throw new ArgumentException(nameof(user));
			}

			return Task.FromResult(user.PasswordHash);

		}

		public Task<bool> HasPasswordAsync(TUser user, CancellationToken cancellationToken)
		{

			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}

			return Task.FromResult(user.PasswordHash != null);

		}
		#endregion

		#region IUserSecurityStampStore
		public Task SetSecurityStampAsync(TUser user, string stamp, CancellationToken cancellationToken)
		{
			if (user == null)
			{
				throw new ArgumentException(nameof(user));
			}

			user.SecurityStamp = stamp;

			return Task.FromResult(0);
		}

		public Task<string> GetSecurityStampAsync(TUser user, CancellationToken cancellationToken)
		{
			if (user == null)
			{
				throw new ArgumentException(nameof(user));
			}

			return Task.FromResult(user.SecurityStamp);
		}
		#endregion

		#region IUserTowFactorStore
		public Task SetTwoFactorEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken)
		{
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}

			user.IsTwoFactorEnabled = enabled;

			return Task.FromResult(0);
		}

		public Task<bool> GetTwoFactorEnabledAsync(TUser user, CancellationToken cancellationToken)
		{
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}

			return Task.FromResult(user.IsTwoFactorEnabled);
		}
		#endregion

		#region IUserEmailStore
		public Task SetEmailAsync(TUser user, string email, CancellationToken cancellationToken)
		{
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}

			if (string.IsNullOrEmpty(email))
			{
				throw new ArgumentNullException(nameof(email));
			}

			user.Email = new ElasticConfirmation(email);

			return Task.FromResult(0);
		}

		public Task<string> GetEmailAsync(TUser user, CancellationToken cancellationToken)
		{
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}

			var email = user.Email?.Value;

			return Task.FromResult(email);
		}

		public Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken cancellationToken)
		{
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}

			if (user.Email == null)
			{
				throw new InvalidOperationException(
					"Cannot get the confirmation status of the e-mail since the user doesn't have an e-mail.");
			}
			return Task.FromResult(user.Email.Confirmed);
		}

		public Task SetEmailConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
		{
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}

			if (user.Email == null)
			{
				throw new InvalidOperationException(
					"Cannot set the confirmation status of the e-mail because user doesn't have an e-mail.");
			}

			user.Email.Confirmed = confirmed;

			user.Email.ConfirmedOn = confirmed
				? DateTimeOffset.Now
				: new DateTimeOffset?();

			return Task.FromResult(0);

		}

		public async Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
		{
			if (string.IsNullOrEmpty(normalizedEmail))
			{
				throw new ArgumentException(nameof(normalizedEmail));
			}

			cancellationToken.ThrowIfCancellationRequested();

			var esResult = await _nestClient.SearchAsync<TUser>(r => r
				.Index(_index)
				.Size(1)
				.Query(q => q
					.Term(t => t.Email.Normalized, normalizedEmail.ToLowerInvariant())
				), cancellationToken
			);

			if (!esResult.IsValid)
			{
				throw new Exception($"ElasticSearch Error in {GetMethodName()}", esResult.OriginalException);
			}

			return esResult.Documents.FirstOrDefault();


		}

		public Task<string> GetNormalizedEmailAsync(TUser user, CancellationToken cancellationToken)
		{
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}

			if (user.Email == null)
			{
				throw new Exception($"The User {user.UserName} does not have email");
			}

			var normalizedEmail = user.Email.Normalized;

			return Task.FromResult(normalizedEmail);
		}

		public Task SetNormalizedEmailAsync(TUser user, string normalizedEmail, CancellationToken cancellationToken)
		{
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}

			if (normalizedEmail != null && user.Email != null)
			{
				user.Email.Normalized = normalizedEmail;
			}

			return Task.FromResult(0);
		}
		#endregion

		#region IUserPhoneNumberStore
		public Task SetPhoneNumberAsync(TUser user, string phoneNumber, CancellationToken cancellationToken)
		{
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}

			if (phoneNumber == null)
			{
				throw new ArgumentNullException(nameof(phoneNumber));
			}

			user.PhoneNumber = new ElasticConfirmation(phoneNumber);

			return Task.FromResult(0);
		}

		public Task<string> GetPhoneNumberAsync(TUser user, CancellationToken cancellationToken)
		{
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}

			return Task.FromResult(user.PhoneNumber?.Value);
		}

		public Task<bool> GetPhoneNumberConfirmedAsync(TUser user, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
		{
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}

			if (user.PhoneNumber == null)
			{
				throw new InvalidOperationException(
					"Cannot get the confirmation status of the phone number since the user doesn't have a phone number.");
			}

			user.PhoneNumber.Confirmed = true;
			user.PhoneNumber.ConfirmedOn = DateTimeOffset.Now;

			return Task.FromResult(user.PhoneNumber.Confirmed);
		}
		#endregion

		#region Private Helpers		
		private async Task<bool> ExistsByIdAsync(string userId, CancellationToken cancellationToken)
		{

			cancellationToken.ThrowIfCancellationRequested();

			var esResult = await _nestClient.GetAsync<TUser>(userId, r => r.Index(_index), cancellationToken);

			if (!esResult.IsValid)
			{
				throw new Exception($"ElasticSearch Error in {GetMethodName()}", esResult.OriginalException);
			}

			return esResult.Source != null;

		}

		private async Task<bool> ExistsByUsernameAsync(string userName, CancellationToken cancellationToken)
		{

			cancellationToken.ThrowIfCancellationRequested();

			var esResult = await _nestClient.SearchAsync<TUser>(r => r
				.Index(_index)
				.Query(q => q
					.Term(t => t.UserName, userName)
				)
				, cancellationToken
			);

			if (!esResult.IsValid)
			{
				throw new Exception($"ElasticSearch Error in {GetMethodName()}", esResult.OriginalException);
			}

			return esResult.Total != 0;

		}
		#endregion

	}
}
