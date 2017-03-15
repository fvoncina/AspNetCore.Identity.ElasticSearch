using Elasticsearch.Net;
using Nest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using AspNetCore.Identity.ElasticSearch;
using System.Threading;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AspNetCore.Identity.ElasticSearch.Tests
{
    public class ElasticStoreTests
    {

        private const string _index = "test_identity_es";
        private readonly IElasticClient _nestClient;

        public ElasticStoreTests()
        {
            var connectionPool = new StaticConnectionPool(new[] { new Uri("http://localhost:9205") })
            {
                SniffedOnStartup = false
            };
            var esConnectionConfiguration = new ConnectionSettings(connectionPool);
            esConnectionConfiguration.DisableDirectStreaming(true);
            _nestClient = new ElasticClient(esConnectionConfiguration);
            _nestClient.DeleteIndex(_index);
        }

        [Fact]
        public async Task Create_Test()
        {

            var store = new ElasticStore<ElasticUser>(_nestClient, _index);

            var user = new ElasticUser
            {
                UserName = "test1",
                PasswordHash = "phash",
                Email = new ElasticEmail("test@test.com")
            };

            var result = await store.CreateAsync(user, CancellationToken.None);

            Assert.Equal(result, IdentityResult.Success);

        }

        [Fact]
        public async Task Create_ShouldThrowDuplicateOnId_Test()
        {
            var ex = await Assert.ThrowsAsync<DuplicateException>(async () =>
            {

                var store = new ElasticStore<ElasticUser>(_nestClient, _index);

                var user = new ElasticUser
                {
                    UserName = "test1",
                    PasswordHash = "phash",
                    Email = new ElasticEmail("test@test.com")
                };

                var result = await store.CreateAsync(user, CancellationToken.None);

                var user2 = new ElasticUser
                {
                    Id = user.Id,
                    UserName = "test1",
                    PasswordHash = "phash",
                    Email = new ElasticEmail("test@test.com")
                };

                await store.CreateAsync(user, CancellationToken.None);

            });

            Assert.NotNull(ex);

        }

        [Fact]
        public async Task Create_ShouldThrowDuplicateOnUsername_Test()
        {
            var ex = await Assert.ThrowsAsync<DuplicateException>(async () =>
            {

                var store = new ElasticStore<ElasticUser>(_nestClient, _index);

                var user = new ElasticUser
                {
                    UserName = "test1",
                    PasswordHash = "phash",
                    Email = new ElasticEmail("test@test.com")
                };

                var result = await store.CreateAsync(user, CancellationToken.None);

                var user2 = new ElasticUser
                {
                    UserName = "test1",
                    PasswordHash = "phash",
                    Email = new ElasticEmail("test@test.com")
                };

                await store.CreateAsync(user, CancellationToken.None);

            });

            Assert.NotNull(ex);

        }

        [Fact]
        public async Task Delete_Test()
        {

            var store = new ElasticStore<ElasticUser>(_nestClient, _index);

            var user = new ElasticUser
            {
                UserName = "test1",
                PasswordHash = "phash",
                Email = new ElasticEmail("test@test.com")
            };

            var createResult = await store.CreateAsync(user, CancellationToken.None);
            var deleteResult = await store.DeleteAsync(user, CancellationToken.None);

            Assert.Equal(createResult, IdentityResult.Success);
            Assert.Equal(deleteResult, IdentityResult.Success);

        }

        [Fact]
        public async Task FindById_Test()
        {

            var store = new ElasticStore<ElasticUser>(_nestClient, _index);

            var user = new ElasticUser
            {
                UserName = "test1",
                PasswordHash = "phash",
                Logins = new List<ElasticUserLogin>
                {
                    new ElasticUserLogin("prov1","key1","name1"),
                    new ElasticUserLogin("prov2","key2","name2")
                },
                Email = new ElasticEmail("test@test.com")
            };

            var createResult = await store.CreateAsync(user, CancellationToken.None);

            var elasticUser = await store.FindByIdAsync(user.Id, CancellationToken.None);

            Assert.Equal(createResult, IdentityResult.Success);
            Assert.Equal(user.Id, elasticUser.Id);

        }

        [Fact]
        public async Task FindByLogin_Test()
        {

            var store = new ElasticStore<ElasticUser>(_nestClient, _index);

            var user1 = new ElasticUser
            {
                UserName = "test1",
                PasswordHash = "phash",
                Logins = new List<ElasticUserLogin> { new ElasticUserLogin(new UserLoginInfo("prov1", "key1", "test1")) },
                Email = new ElasticEmail("test1@test.com")
            };
            var user2 = new ElasticUser
            {
                UserName = "test2",
                PasswordHash = "phash",
                Logins = new List<ElasticUserLogin> { new ElasticUserLogin(new UserLoginInfo("prov2", "key2", "test2")) },
                Email = new ElasticEmail("test2@test.com")
            };

            var createResult1 = await store.CreateAsync(user1, CancellationToken.None);
            var createResult2 = await store.CreateAsync(user2, CancellationToken.None);
            var elasticUser = await store.FindByLoginAsync("prov2", "key2", CancellationToken.None);

            Assert.Equal(createResult1, IdentityResult.Success);
            Assert.Equal(createResult2, IdentityResult.Success);
            Assert.Equal(user2.Id, elasticUser.Id);

        }

        [Fact]
        public async Task FindByName_Test()
        {

            var store = new ElasticStore<ElasticUser>(_nestClient, _index);

            var user = new ElasticUser
            {
                UserName = "test1",
                PasswordHash = "phash",
                Logins = new List<ElasticUserLogin> { new ElasticUserLogin(new UserLoginInfo("prov", "key", "test")) },
                Email = new ElasticEmail("test@test.com")
            };

            var createResult = await store.CreateAsync(user, CancellationToken.None);
            var elasticUser = await store.FindByNameAsync("test1", CancellationToken.None);

            Assert.Equal(createResult, IdentityResult.Success);
            Assert.Equal(user.Id, elasticUser.Id);

        }

        [Fact]
        public async Task GetLogins_Test()
        {

            var store = new ElasticStore<ElasticUser>(_nestClient, _index);

            var user = new ElasticUser
            {
                UserName = "test1",
                PasswordHash = "phash",
                Logins = new List<ElasticUserLogin> {
                    new ElasticUserLogin(new UserLoginInfo("prov1", "key", "test1")),
                    new ElasticUserLogin(new UserLoginInfo("prov2", "key2", "test1"))
                },
                Email = new ElasticEmail("test@test.com")
            };

            var logins = await store.GetLoginsAsync(user, CancellationToken.None);

            Assert.Equal(2, logins.Count);

        }

        [Fact]
        public async Task GetNormalizedUserName_Test()
        {

            var store = new ElasticStore<ElasticUser>(_nestClient, _index);

            const string expected = "test1";
            var user = new ElasticUser
            {
                UserName = "test1",
                PasswordHash = "phash",
                Logins = new List<ElasticUserLogin> {
                    new ElasticUserLogin(new UserLoginInfo("prov1", "key", "test1")),
                    new ElasticUserLogin(new UserLoginInfo("prov2", "key2", "test1"))
                },
                Email = new ElasticEmail("test@test.com")
            };
            var actual = await store.GetNormalizedUserNameAsync(user, CancellationToken.None);

            Assert.Equal(expected, actual);

        }

        [Fact]
        public async Task GetUserId_Test()
        {

            var store = new ElasticStore<ElasticUser>(_nestClient, _index);

            const string expected = "1";
            var user = new ElasticUser
            {
                Id = "1",
                UserName = "test1",
                PasswordHash = "phash",
                Logins = new List<ElasticUserLogin> {
                    new ElasticUserLogin(new UserLoginInfo("prov1", "key", "test1")),
                    new ElasticUserLogin(new UserLoginInfo("prov2", "key2", "test1"))
                },
                Email = new ElasticEmail("test@test.com")
            };
            var actual = await store.GetUserIdAsync(user, CancellationToken.None);

            Assert.Equal(expected, actual);

        }

        [Fact]
        public async Task GetUserName_Test()
        {

            var store = new ElasticStore<ElasticUser>(_nestClient, _index);

            const string expected = "test1";
            var user = new ElasticUser
            {
                Id = "test1",
                UserName = "test1",
                PasswordHash = "phash",
                Logins = new List<ElasticUserLogin> {
                    new ElasticUserLogin(new UserLoginInfo("prov1", "key", "test1")),
                    new ElasticUserLogin(new UserLoginInfo("prov2", "key2", "test1"))
                },
                Email = new ElasticEmail("test@test.com")
            };
            var actual = await store.GetUserNameAsync(user, CancellationToken.None);

            Assert.Equal(expected, actual);

        }

        [Fact]
        public async Task RemoveLogin_Test()
        {

            var store = new ElasticStore<ElasticUser>(_nestClient, _index);

            var user = new ElasticUser
            {
                UserName = "test1",
                PasswordHash = "phash",
                Logins = new List<ElasticUserLogin> {
                    new ElasticUserLogin(new UserLoginInfo("prov1", "key1", "test1")),
                    new ElasticUserLogin(new UserLoginInfo("prov2", "key2", "test2"))
                },
                Email = new ElasticEmail("test@test.com")
            };

            var createResult = await store.CreateAsync(user, CancellationToken.None);
            await store.RemoveLoginAsync(user, "prov1", "key1", CancellationToken.None);
            var elasticUser = await store.FindByIdAsync(user.Id, CancellationToken.None);

            Assert.Equal(createResult, IdentityResult.Success);
            Assert.Equal(elasticUser.Logins.Count, 1);

        }

        [Fact]
        public async Task SetUserName_Test()
        {

            var store = new ElasticStore<ElasticUser>(_nestClient, _index);

            var user = new ElasticUser
            {
                UserName = "test1",
                PasswordHash = "phash",
                Logins = new List<ElasticUserLogin> {
                    new ElasticUserLogin(new UserLoginInfo("prov1", "key1", "test1")),
                    new ElasticUserLogin(new UserLoginInfo("prov2", "key2", "test2"))
                },
                Email = new ElasticEmail("test@test.com")
            };

            var createResult = await store.CreateAsync(user, CancellationToken.None);
            await store.SetUserNameAsync(user, "test11", CancellationToken.None);

            var elasticUser = await store.FindByIdAsync(user.Id, CancellationToken.None);

            Assert.Equal(createResult, IdentityResult.Success);
            Assert.Equal(user.UserName, elasticUser.UserName);

        }

        [Fact]
        public async Task Update_Test()
        {

            var store = new ElasticStore<ElasticUser>(_nestClient, _index);

            var user = new ElasticUser
            {
                UserName = "test1",
                PasswordHash = "phash",
                Logins = new List<ElasticUserLogin> {
                    new ElasticUserLogin(new UserLoginInfo("prov1", "key1", "test1")),
                    new ElasticUserLogin(new UserLoginInfo("prov2", "key2", "test2"))
                },
                Email = new ElasticEmail("test@test.com")
            };

            var createResult = await store.CreateAsync(user, CancellationToken.None);

            user.Email.Email = "test1@test.com";

            await store.UpdateAsync(user, CancellationToken.None);

            var elasticUser = await store.FindByIdAsync(user.Id, CancellationToken.None);

            Assert.Equal(createResult, IdentityResult.Success);
            Assert.Equal(user.Email.Email, elasticUser.Email.Email);

        }

        [Fact]
        public async Task AddLogin_ShouldAdd_Test()
        {

            var store = new ElasticStore<ElasticUser>(_nestClient, _index);

            var user = new ElasticUser
            {
                UserName = "test1",
                PasswordHash = "phash",
                Logins = new List<ElasticUserLogin> {
                    new ElasticUserLogin(new UserLoginInfo("prov1", "key1", "test1"))
                },
                Email = new ElasticEmail("test@test.com")
            };

            var createResult = await store.CreateAsync(user, CancellationToken.None);

            await store.AddLoginAsync(user, new UserLoginInfo("prov2", "key2", "test2"), CancellationToken.None);

            var elasticUser = await store.FindByIdAsync(user.Id, CancellationToken.None);

            Assert.Equal(createResult, IdentityResult.Success);
            Assert.Equal(user.Logins.Count, 2);

        }

        [Fact]
        public async Task AddLogin_ShouldNotAdd_Test()
        {

            var store = new ElasticStore<ElasticUser>(_nestClient, _index);

            var user = new ElasticUser
            {
                UserName = "test1",
                PasswordHash = "phash",
                Logins = new List<ElasticUserLogin> {
                    new ElasticUserLogin(new UserLoginInfo("prov1", "key1", "test1"))
                },
                Email = new ElasticEmail("test@test.com")
            };

            var createResult = await store.CreateAsync(user, CancellationToken.None);

            await store.AddLoginAsync(user, new UserLoginInfo("prov1", "key1", "test1"), CancellationToken.None);

            var elasticUser = await store.FindByIdAsync(user.Id, CancellationToken.None);

            Assert.Equal(createResult, IdentityResult.Success);
            Assert.Equal(user.Logins.Count, 1);

        }

        [Fact]
        public async Task GetClaims_Test()
        {

            var store = new ElasticStore<ElasticUser>(_nestClient, _index);

            var user = new ElasticUser
            {
                UserName = "test1",
                PasswordHash = "phash",
                Logins = new List<ElasticUserLogin> {
                    new ElasticUserLogin(new UserLoginInfo("prov1", "key1", "test1"))
                },
                Claims = new List<ElasticClaim> {
                    new ElasticClaim("type1","value1"),
                    new ElasticClaim("type2","value2")
                },
                Email = new ElasticEmail("test@test.com")
            };

            var createResult = await store.CreateAsync(user, CancellationToken.None);

            var elasticUser = await store.FindByIdAsync(user.Id, CancellationToken.None);

            var claims = await store.GetClaimsAsync(elasticUser, CancellationToken.None);

            Assert.Equal(createResult, IdentityResult.Success);
            Assert.Equal(claims.Count, 2);

        }

        [Fact]
        public async Task AddClaims_Test()
        {

            var store = new ElasticStore<ElasticUser>(_nestClient, _index);

            var user = new ElasticUser
            {
                UserName = "test1",
                PasswordHash = "phash",
                Logins = new List<ElasticUserLogin> {
                    new ElasticUserLogin(new UserLoginInfo("prov1", "key1", "test1"))
                },
                Claims = new List<ElasticClaim> {
                    new ElasticClaim("type1","value1"),
                    new ElasticClaim("type2","value2")
                },
                Email = new ElasticEmail("test@test.com")
            };

            var createResult = await store.CreateAsync(user, CancellationToken.None);

            var newClaims = new List<Claim>
            {
                new Claim("type2","value2"),
                new Claim("type3","value3")
            };
            await store.AddClaimsAsync(user, newClaims, CancellationToken.None);

            var elasticUser = await store.FindByIdAsync(user.Id, CancellationToken.None);

            Assert.Equal(createResult, IdentityResult.Success);
            Assert.Equal(elasticUser.Claims.Count, 3);

        }

        [Fact]
        public async Task ReplaceClaim_Test()
        {

            var store = new ElasticStore<ElasticUser>(_nestClient, _index);

            var user = new ElasticUser
            {
                UserName = "test1",
                PasswordHash = "phash",
                Logins = new List<ElasticUserLogin> {
                    new ElasticUserLogin(new UserLoginInfo("prov1", "key1", "test1"))
                },
                Claims = new List<ElasticClaim> {
                    new ElasticClaim("type1","value1"),
                    new ElasticClaim("type2","value2")
                },
                Email = new ElasticEmail("test@test.com")
            };

            var createResult = await store.CreateAsync(user, CancellationToken.None);

            await store.ReplaceClaimAsync(user, new Claim("type2", "value2"), new Claim("type2", "value22"), CancellationToken.None);

            var elasticUser = await store.FindByIdAsync(user.Id, CancellationToken.None);

            var changedClaim = elasticUser.Claims.Find(x => x.ClaimValue == "value22");

            Assert.Equal(createResult, IdentityResult.Success);
            Assert.NotNull(changedClaim);

        }

        [Fact]
        public async Task RemoveClaim_Test()
        {

            var store = new ElasticStore<ElasticUser>(_nestClient, _index);

            var user = new ElasticUser
            {
                UserName = "test1",
                PasswordHash = "phash",
                Logins = new List<ElasticUserLogin> {
                    new ElasticUserLogin(new UserLoginInfo("prov1", "key1", "test1"))
                },
                Claims = new List<ElasticClaim> {
                    new ElasticClaim("type1","value1"),
                    new ElasticClaim("type2","value2")
                },
                Email = new ElasticEmail("test@test.com")
            };

            var createResult = await store.CreateAsync(user, CancellationToken.None);

            var claimsToBeRemoved = new List<Claim>
            {
                new Claim("type2","value2")
            };

            await store.RemoveClaimsAsync(user, claimsToBeRemoved, CancellationToken.None);

            var elasticUser = await store.FindByIdAsync(user.Id, CancellationToken.None);

            Assert.Equal(createResult, IdentityResult.Success);
            Assert.Equal(elasticUser.Claims.Count, 2);

        }

        [Fact]
        public async Task GetUsersForClaim_Test()
        {

            var store = new ElasticStore<ElasticUser>(_nestClient, _index);

            var user1 = new ElasticUser
            {
                UserName = "test1",
                PasswordHash = "phash",
                Logins = new List<ElasticUserLogin> { new ElasticUserLogin(new UserLoginInfo("prov1", "key1", "test1")) },
                Claims = new List<ElasticClaim> {
                    new ElasticClaim("type", "value1")
                },
                Email = new ElasticEmail("test1@test.com")
            };
            var user2 = new ElasticUser
            {
                UserName = "test2",
                PasswordHash = "phash",
                Logins = new List<ElasticUserLogin> { new ElasticUserLogin(new UserLoginInfo("prov2", "key2", "test2")) },
                Claims = new List<ElasticClaim> {
                    new ElasticClaim("type", "value2")
                },
                Email = new ElasticEmail("test2@test.com")
            };

            var createResult1 = await store.CreateAsync(user1, CancellationToken.None);
            var createResult2 = await store.CreateAsync(user2, CancellationToken.None);
            var elasticUser = await store.FindByLoginAsync("prov2", "key2", CancellationToken.None);

            var foundByClaim = await store.GetUsersForClaimAsync(new Claim("type", "value2"), CancellationToken.None);

            Assert.Equal(createResult1, IdentityResult.Success);
            Assert.Equal(createResult2, IdentityResult.Success);
            Assert.Equal(foundByClaim.Count, 1);

        }


        [Fact]
        public async Task AddToRole_Test()
        {

            var store = new ElasticStore<ElasticUser>(_nestClient, _index);

            var user = new ElasticUser
            {
                UserName = "test1",
                PasswordHash = "phash",
                Logins = new List<ElasticUserLogin> { new ElasticUserLogin(new UserLoginInfo("prov1", "key1", "test1")) },
                Claims = new List<ElasticClaim> {
                    new ElasticClaim("type", "value1")
                },
                Email = new ElasticEmail("test1@test.com")
            };

            var createResult = await store.CreateAsync(user, CancellationToken.None);

            await store.AddToRoleAsync(user, "role1", CancellationToken.None);

            var elasticUser = await store.FindByIdAsync(user.Id, CancellationToken.None);

            var isInRole = await store.IsInRoleAsync(elasticUser, "role1", CancellationToken.None);

            Assert.Equal(createResult, IdentityResult.Success);
            Assert.True(isInRole);

        }

        [Fact]
        public async Task GetUsersInRole_Test()
        {

            var store = new ElasticStore<ElasticUser>(_nestClient, _index);

            var user1 = new ElasticUser
            {
                UserName = "test1",
                PasswordHash = "phash",
                Logins = new List<ElasticUserLogin> { new ElasticUserLogin(new UserLoginInfo("prov1", "key1", "test1")) },
                Claims = new List<ElasticClaim> {
                    new ElasticClaim("type", "value1")
                },
                Email = new ElasticEmail("test1@test.com")
            };
            var user2 = new ElasticUser
            {
                UserName = "test2",
                PasswordHash = "phash",
                Logins = new List<ElasticUserLogin> { new ElasticUserLogin(new UserLoginInfo("prov2", "key2", "test2")) },
                Claims = new List<ElasticClaim> {
                    new ElasticClaim("type", "value2")
                },
                Email = new ElasticEmail("test2@test.com")
            };

            var createResult1 = await store.CreateAsync(user1, CancellationToken.None);
            var createResult2 = await store.CreateAsync(user2, CancellationToken.None);

            await store.AddToRoleAsync(user1, "role1", CancellationToken.None);
            await store.AddToRoleAsync(user1, "role2", CancellationToken.None);
            await store.AddToRoleAsync(user2, "role1", CancellationToken.None);

            var usersForRole1 = await store.GetUsersInRoleAsync("role1", CancellationToken.None);
            var usersForRole2 = await store.GetUsersInRoleAsync("role2", CancellationToken.None);

            Assert.Equal(createResult1, IdentityResult.Success);
            Assert.Equal(createResult2, IdentityResult.Success);
            Assert.Equal(usersForRole1.Count, 2);
            Assert.Equal(usersForRole2.Count, 1);

        }

        [Fact]
        public async Task SetSecurityStamp_Test()
        {

            var store = new ElasticStore<ElasticUser>(_nestClient, _index);

            var user = new ElasticUser
            {
                UserName = "test1",
                PasswordHash = "phash",
                Logins = new List<ElasticUserLogin> { new ElasticUserLogin(new UserLoginInfo("prov1", "key1", "test1")) },
                Claims = new List<ElasticClaim> {
                    new ElasticClaim("type", "value1")
                },
                Email = new ElasticEmail("test1@test.com")
            };

            var createResult = await store.CreateAsync(user, CancellationToken.None);

            await store.SetSecurityStampAsync(user, "stamp", CancellationToken.None);

            var elasticUser = await store.FindByIdAsync(user.Id, CancellationToken.None);

            Assert.Equal(createResult, IdentityResult.Success);
            Assert.Equal(elasticUser.SecurityStamp, "stamp");

        }


    }
}
