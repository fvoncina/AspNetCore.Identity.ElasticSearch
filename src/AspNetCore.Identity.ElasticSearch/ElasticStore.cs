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
    public class ElasticStore<TUser> :
        IUserStore<TUser>,
        IUserLoginStore<TUser>,
        IUserClaimStore<TUser>,
        IUserRoleStore<TUser>,
        IUserPasswordStore<TUser>,
        IUserSecurityStampStore<TUser>
        where TUser : ElasticUser

    {

        #region Private Vars

        private readonly string _index;
        private readonly IElasticClient _nestClient;
        private readonly int _defaultQuerySize;
        private readonly int _defaultShards;
        private readonly int _defaultReplicas;

        #endregion

        #region ctor

        public ElasticStore(IElasticClient nestClient, string index, int defaultQuerySize = 1000, int defaultShards = 1, int defaultReplicas = 0)
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
            else if (await ExistsByIdAsync(user.Id, cancellationToken))
            {
                throw new DuplicateException($"A user with id {user.Id} already exists");
            }

            if (await ExistsByUsernameAsync(user.UserName, cancellationToken))
            {
                throw new DuplicateException($"A user with userName {user.UserName} already exists");
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
                .Size(_defaultQuerySize)
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
                .Size(_defaultQuerySize)
                .Query(q => q
                    .Term(t => t.UserName, normalizedUserName)
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

            return Task.FromResult(user.NormalizedUserName);

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

        public async Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
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

            cancellationToken.ThrowIfCancellationRequested();

            var elasticUser = await FindByIdAsync(user.Id, cancellationToken);

            var login = new UserLoginInfo(loginProvider, providerKey, string.Empty);

            elasticUser.Logins.RemoveAll(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey);

            var esResult = await _nestClient.IndexAsync<TUser>(elasticUser, r => r
                .Index(_index)
                .Refresh(Elasticsearch.Net.Refresh.True)
                , cancellationToken
            );

            if (!esResult.IsValid)
            {
                throw new Exception($"ElasticSearch Error in {GetMethodName()}", esResult.OriginalException);
            }

        }

        public async Task SetNormalizedUserNameAsync(TUser user, string normalizedName, CancellationToken cancellationToken)
        {
            await SetUserNameAsync(user, normalizedName, cancellationToken);
        }

        public async Task SetUserNameAsync(TUser user, string userName, CancellationToken cancellationToken)
        {

            if (user == null)
            {
                throw new ArgumentException(nameof(user));
            }

            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentException(nameof(userName));
            }

            cancellationToken.ThrowIfCancellationRequested();

            user.UserName = userName;

            var esResult = await _nestClient.IndexAsync(user, r => r
                 .Index(_index)
                 .Refresh(Elasticsearch.Net.Refresh.True)
            );

            if (!esResult.IsValid)
            {
                throw new Exception($"ElasticSearch Error in {GetMethodName()}", esResult.OriginalException);
            }

        }

        public async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken)
        {

            if (user == null)
            {
                throw new ArgumentException(nameof(user));
            }

            cancellationToken.ThrowIfCancellationRequested();

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

        public async Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken)
        {

            if (user == null)
            {
                throw new ArgumentException(nameof(user));
            }

            if (login == null)
            {
                throw new ArgumentException(nameof(login));
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (!user.Logins.Any(x => x.LoginProvider == login.LoginProvider && x.ProviderKey == login.ProviderKey))
            {

                user.Logins.Add(new ElasticUserLogin(login));

                var esResult = await _nestClient.IndexAsync(user, r => r
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

        public async Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {

            if (user == null)
            {
                throw new ArgumentException(nameof(user));
            }

            if (claims == null)
            {
                throw new ArgumentException(nameof(claims));
            }

            cancellationToken.ThrowIfCancellationRequested();

            foreach (var newClaim in claims)
            {
                var currentClaim = user.Claims.Find(x => x.ClaimType == newClaim.Type);
                if (currentClaim != null)
                {
                    user.Claims.Remove(currentClaim);
                }
                user.Claims.Add(new ElasticClaim(newClaim));
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

        }

        public async Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
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

            cancellationToken.ThrowIfCancellationRequested();

            var currentClaim = user.Claims.Find(x => x.ClaimType == claim.Type);

            if (currentClaim != null)
            {
                user.Claims.Remove(currentClaim);
            }

            user.Claims.Add(new ElasticClaim(newClaim));

            var esResult = await _nestClient.IndexAsync(user, r => r
                .Index(_index)
                .Refresh(Elasticsearch.Net.Refresh.True)
                , cancellationToken
            );

            if (!esResult.IsValid)
            {
                throw new Exception($"ElasticSearch Error in {GetMethodName()}", esResult.OriginalException);
            }

        }

        public async Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {

            if (user == null)
            {
                throw new ArgumentException(nameof(user));
            }

            if (claims == null)
            {
                throw new ArgumentException(nameof(claims));
            }

            cancellationToken.ThrowIfCancellationRequested();

            foreach (var newClaim in claims)
            {

                var currentClaim = user.Claims.Find(x => x == newClaim);

                if (currentClaim != null)
                {
                    user.Claims.Remove(currentClaim);
                }

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
                                    m => m.Term(_nestClient.Infer.NestedProperty<ElasticUser, ElasticClaim>(x => x.Claims, x => x.ClaimType), claim.Type),
                                    m => m.Term(_nestClient.Infer.NestedProperty<ElasticUser, ElasticClaim>(x => x.Claims, x => x.ClaimValue), claim.Value)
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

            cancellationToken.ThrowIfCancellationRequested();

            user.Roles.Add(roleName);

            var esResult = await _nestClient.IndexAsync(user, r => r
                .Index(_index)
                .Refresh(Elasticsearch.Net.Refresh.True)
                , cancellationToken
            );

            if (!esResult.IsValid)
            {
                throw new Exception($"ElasticSearch Error in {GetMethodName()}", esResult.OriginalException);
            }

        }

        public async Task RemoveFromRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {

            if (user == null)
            {
                throw new ArgumentException(nameof(user));
            }

            cancellationToken.ThrowIfCancellationRequested();

            user.Roles.Remove(roleName);

            var esResult = await _nestClient.IndexAsync(user, r => r
                .Index(_index)
                .Refresh(Elasticsearch.Net.Refresh.True)
                , cancellationToken
            );

            if (!esResult.IsValid)
            {
                throw new Exception($"ElasticSearch Error in {GetMethodName()}", esResult.OriginalException);
            }

        }

        public Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken)
        {

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var result = user.Roles.ToList();

            return Task.FromResult((IList<string>)result);

        }

        public Task<bool> IsInRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (roleName == null)
            {
                throw new ArgumentNullException(nameof(roleName));
            }

            return Task.FromResult(user.Roles.Contains(roleName));

        }

        public async Task<IList<TUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {

            if (string.IsNullOrEmpty(roleName))
            {
                throw new ArgumentException(nameof(roleName));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var esResult = await _nestClient.SearchAsync<TUser>(r => r
                .Index(_index)
                .Size(_defaultQuerySize)
                .Query(q => q
                    .Term(t => t.Roles, roleName)
                ), cancellationToken
            );

            if (!esResult.IsValid)
            {
                throw new Exception($"ElasticSearch Error in {GetMethodName()}", esResult.OriginalException);
            }

            return (IList<TUser>)esResult.Documents.ToList();

        }

        #endregion

        #region IUserPasswordStore

        public async Task SetPasswordHashAsync(TUser user, string passwordHash, CancellationToken cancellationToken)
        {

            if (user == null)
            {
                throw new ArgumentException(nameof(user));
            }

            cancellationToken.ThrowIfCancellationRequested();

            user.PasswordHash = passwordHash;

            var esResult = await _nestClient.IndexAsync(user, r => r
                .Index(_index)
                .Refresh(Elasticsearch.Net.Refresh.True)
                , cancellationToken
            );

            if (!esResult.IsValid)
            {
                throw new Exception($"ElasticSearch Error in {GetMethodName()}", esResult.OriginalException);
            }

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

        public async Task SetSecurityStampAsync(TUser user, string stamp, CancellationToken cancellationToken)
        {

            if (user == null)
            {
                throw new ArgumentException(nameof(user));
            }

            cancellationToken.ThrowIfCancellationRequested();

            user.SecurityStamp = stamp;

            var esResult = await _nestClient.IndexAsync(user, r => r
                .Index(_index)
                .Refresh(Elasticsearch.Net.Refresh.True)
                , cancellationToken
            );

            if (!esResult.IsValid)
            {
                throw new Exception($"ElasticSearch Error in {GetMethodName()}", esResult.OriginalException);
            }

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

        #region Private Helpers

        private void EnsureInitialization()
        {

            var createDescriptor = new CreateIndexDescriptor(_index)
                .Settings(s => s
                    .NumberOfShards(_defaultShards)
                    .NumberOfReplicas(_defaultReplicas)
                ).Mappings(m => m
                    .Map<TUser>(mm => mm
                        .AutoMap()
                    )
                );

            var createIndexResult = _nestClient.CreateIndex(createDescriptor);

            if (!createIndexResult.IsValid)
            {
                throw new InvalidOperationException($"Error creating index {_index}", createIndexResult.OriginalException);

            }

            //_nestClient.Map<ElasticUser>(m => m.AutoMap().Index(_index));

        }

        private string GetMethodName([CallerMemberName] string callerMemberName = null)
        {
            return callerMemberName;
        }

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
