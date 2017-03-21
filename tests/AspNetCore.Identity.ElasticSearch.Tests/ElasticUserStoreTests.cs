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
	public class ElasticUserStoreTests : BaseStoreTests
	{

		public ElasticUserStoreTests() : base()
		{
		}

		[Fact]
		public async Task Create_Test()
		{

			var store = new ElasticUserStore<ElasticUser, ElasticRole>(_nestClient, _index);

			var user = new ElasticUser
			{
				UserName = "test1",
				PasswordHash = "phash",
				Email = new ElasticConfirmation("test@test.com")
			};

			var result = await store.CreateAsync(user, NoCancellationToken);

			Assert.Equal(result, IdentityResult.Success);

		}

		[Fact]
		public async Task Delete_Test()
		{

			var store = new ElasticUserStore<ElasticUser, ElasticRole>(_nestClient, _index);

			var user = new ElasticUser
			{
				UserName = "test1",
				PasswordHash = "phash",
				Email = new ElasticConfirmation("test@test.com")
			};

			var createResult = await store.CreateAsync(user, NoCancellationToken);
			var deleteResult = await store.DeleteAsync(user, NoCancellationToken);

			Assert.Equal(createResult, IdentityResult.Success);
			Assert.Equal(deleteResult, IdentityResult.Success);

		}

		[Fact]
		public async Task FindById_Test()
		{

			var store = new ElasticUserStore<ElasticUser, ElasticRole>(_nestClient, _index);

			var user = new ElasticUser
			{
				UserName = "test1",
				PasswordHash = "phash",
				Logins = new List<ElasticUserLogin>
				{
					new ElasticUserLogin("prov1","key1","name1"),
					new ElasticUserLogin("prov2","key2","name2")
				},
				Email = new ElasticConfirmation("test@test.com")
			};

			var createResult = await store.CreateAsync(user, NoCancellationToken);

			var elasticUser = await store.FindByIdAsync(user.Id, NoCancellationToken);

			Assert.Equal(createResult, IdentityResult.Success);
			Assert.Equal(user.Id, elasticUser.Id);

		}

		[Fact]
		public async Task FindByLogin_Test()
		{

			var store = new ElasticUserStore<ElasticUser, ElasticRole>(_nestClient, _index);

			var user1 = new ElasticUser
			{
				UserName = "test1",
				PasswordHash = "phash",
				Logins = new List<ElasticUserLogin> { new ElasticUserLogin(new UserLoginInfo("prov1", "key1", "test1")) },
				Email = new ElasticConfirmation("test1@test.com")
			};
			var user2 = new ElasticUser
			{
				UserName = "test2",
				PasswordHash = "phash",
				Logins = new List<ElasticUserLogin> { new ElasticUserLogin(new UserLoginInfo("prov2", "key2", "test2")) },
				Email = new ElasticConfirmation("test2@test.com")
			};

			var createResult1 = await store.CreateAsync(user1, NoCancellationToken);
			var createResult2 = await store.CreateAsync(user2, NoCancellationToken);
			var elasticUser = await store.FindByLoginAsync("prov2", "key2", NoCancellationToken);

			Assert.Equal(createResult1, IdentityResult.Success);
			Assert.Equal(createResult2, IdentityResult.Success);
			Assert.Equal(user2.Id, elasticUser.Id);

		}

		[Fact]
		public async Task FindByName_Test()
		{

			var store = new ElasticUserStore<ElasticUser, ElasticRole>(_nestClient, _index);

			var user = new ElasticUser
			{
				UserName = "test1",
				PasswordHash = "phash",
				Logins = new List<ElasticUserLogin> { new ElasticUserLogin(new UserLoginInfo("prov", "key", "test")) },
				Email = new ElasticConfirmation("test@test.com")
			};

			var createResult = await store.CreateAsync(user, NoCancellationToken);
			var elasticUser = await store.FindByNameAsync("test1", NoCancellationToken);

			Assert.Equal(createResult, IdentityResult.Success);
			Assert.Equal(user.Id, elasticUser.Id);

		}

		[Fact]
		public async Task GetLogins_Test()
		{

			var store = new ElasticUserStore<ElasticUser, ElasticRole>(_nestClient, _index);

			var user = new ElasticUser
			{
				UserName = "test1",
				PasswordHash = "phash",
				Logins = new List<ElasticUserLogin> {
					new ElasticUserLogin(new UserLoginInfo("prov1", "key", "test1")),
					new ElasticUserLogin(new UserLoginInfo("prov2", "key2", "test1"))
				},
				Email = new ElasticConfirmation("test@test.com")
			};

			var logins = await store.GetLoginsAsync(user, NoCancellationToken);

			Assert.Equal(2, logins.Count);

		}

		[Fact]
		public async Task GetNormalizedUserName_Test()
		{

			var store = new ElasticUserStore<ElasticUser, ElasticRole>(_nestClient, _index);

			const string expected = "test1";
			var user = new ElasticUser
			{
				UserName = "test1",
				PasswordHash = "phash",
				Logins = new List<ElasticUserLogin> {
					new ElasticUserLogin(new UserLoginInfo("prov1", "key", "test1")),
					new ElasticUserLogin(new UserLoginInfo("prov2", "key2", "test1"))
				},
				Email = new ElasticConfirmation("test@test.com")
			};
			var actual = await store.GetNormalizedUserNameAsync(user, NoCancellationToken);

			Assert.Equal(expected, actual);

		}

		[Fact]
		public async Task GetUserId_Test()
		{

			var store = new ElasticUserStore<ElasticUser, ElasticRole>(_nestClient, _index);

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
				Email = new ElasticConfirmation("test@test.com")
			};
			var actual = await store.GetUserIdAsync(user, NoCancellationToken);

			Assert.Equal(expected, actual);

		}

		[Fact]
		public async Task GetUserName_Test()
		{

			var store = new ElasticUserStore<ElasticUser, ElasticRole>(_nestClient, _index);

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
				Email = new ElasticConfirmation("test@test.com")
			};
			var actual = await store.GetUserNameAsync(user, NoCancellationToken);

			Assert.Equal(expected, actual);

		}

		[Fact]
		public async Task RemoveLogin_Test()
		{

			var store = new ElasticUserStore<ElasticUser, ElasticRole>(_nestClient, _index);
			var user = new ElasticUser
			{
				UserName = "test1",
				PasswordHash = "phash",
				Logins = new List<ElasticUserLogin> {
					new ElasticUserLogin(new UserLoginInfo("prov1", "key1", "test1")),
					new ElasticUserLogin(new UserLoginInfo("prov2", "key2", "test2"))
				},
				Email = new ElasticConfirmation("test@test.com")
			};
			var createResult = await store.CreateAsync(user, NoCancellationToken);
			await store.RemoveLoginAsync(user, "prov1", "key1", NoCancellationToken);
			await store.UpdateAsync(user, NoCancellationToken);
			var elasticUser = await store.FindByIdAsync(user.Id, NoCancellationToken);

			Assert.Equal(createResult, IdentityResult.Success);
			Assert.Equal(elasticUser.Logins.Count, 1);

		}

		[Fact]
		public async Task SetUserName_Test()
		{

			var store = new ElasticUserStore<ElasticUser, ElasticRole>(_nestClient, _index);
			var user = new ElasticUser
			{
				UserName = "test1",
				PasswordHash = "phash",
				Logins = new List<ElasticUserLogin> {
					new ElasticUserLogin(new UserLoginInfo("prov1", "key1", "test1")),
					new ElasticUserLogin(new UserLoginInfo("prov2", "key2", "test2"))
				},
				Email = new ElasticConfirmation("test@test.com")
			};
			var createResult = await store.CreateAsync(user, NoCancellationToken);
			await store.SetUserNameAsync(user, "test11", NoCancellationToken);
			await store.UpdateAsync(user, NoCancellationToken);
			var elasticUser = await store.FindByIdAsync(user.Id, NoCancellationToken);

			Assert.Equal(createResult, IdentityResult.Success);
			Assert.Equal(user.UserName, elasticUser.UserName);

		}

		[Fact]
		public async Task Update_Test()
		{

			var store = new ElasticUserStore<ElasticUser, ElasticRole>(_nestClient, _index);

			var user = new ElasticUser
			{
				UserName = "test1",
				PasswordHash = "phash",
				Logins = new List<ElasticUserLogin> {
					new ElasticUserLogin(new UserLoginInfo("prov1", "key1", "test1")),
					new ElasticUserLogin(new UserLoginInfo("prov2", "key2", "test2"))
				},
				Email = new ElasticConfirmation("test@test.com")
			};

			var createResult = await store.CreateAsync(user, NoCancellationToken);

			user.Email.Value = "test1@test.com";

			await store.UpdateAsync(user, NoCancellationToken);

			var elasticUser = await store.FindByIdAsync(user.Id, NoCancellationToken);

			Assert.Equal(createResult, IdentityResult.Success);
			Assert.Equal(user.Email.Value, elasticUser.Email.Value);

		}

		[Fact]
		public async Task AddLogin_ShouldAdd_Test()
		{

			var store = new ElasticUserStore<ElasticUser, ElasticRole>(_nestClient, _index);

			var user = new ElasticUser
			{
				UserName = "test1",
				PasswordHash = "phash",
				Logins = new List<ElasticUserLogin> {
					new ElasticUserLogin(new UserLoginInfo("prov1", "key1", "test1"))
				},
				Email = new ElasticConfirmation("test@test.com")
			};

			var createResult = await store.CreateAsync(user, NoCancellationToken);

			await store.AddLoginAsync(user, new UserLoginInfo("prov2", "key2", "test2"), NoCancellationToken);

			var elasticUser = await store.FindByIdAsync(user.Id, NoCancellationToken);

			Assert.Equal(createResult, IdentityResult.Success);
			Assert.Equal(user.Logins.Count, 2);

		}

		[Fact]
		public async Task AddLogin_ShouldNotAdd_Test()
		{

			var store = new ElasticUserStore<ElasticUser, ElasticRole>(_nestClient, _index);

			var user = new ElasticUser
			{
				UserName = "test1",
				PasswordHash = "phash",
				Logins = new List<ElasticUserLogin> {
					new ElasticUserLogin(new UserLoginInfo("prov1", "key1", "test1"))
				},
				Email = new ElasticConfirmation("test@test.com")
			};

			var createResult = await store.CreateAsync(user, NoCancellationToken);

			await store.AddLoginAsync(user, new UserLoginInfo("prov1", "key1", "test1"), NoCancellationToken);

			var elasticUser = await store.FindByIdAsync(user.Id, NoCancellationToken);

			Assert.Equal(createResult, IdentityResult.Success);
			Assert.Equal(user.Logins.Count, 1);

		}

		[Fact]
		public async Task GetClaims_Test()
		{

			var store = new ElasticUserStore<ElasticUser, ElasticRole>(_nestClient, _index);

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
				Email = new ElasticConfirmation("test@test.com")
			};

			var createResult = await store.CreateAsync(user, NoCancellationToken);

			var elasticUser = await store.FindByIdAsync(user.Id, NoCancellationToken);

			var claims = await store.GetClaimsAsync(elasticUser, NoCancellationToken);

			Assert.Equal(createResult, IdentityResult.Success);
			Assert.Equal(claims.Count, 2);

		}

		[Fact]
		public async Task AddClaims_Test()
		{

			var store = new ElasticUserStore<ElasticUser, ElasticRole>(_nestClient, _index);

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
				Email = new ElasticConfirmation("test@test.com")
			};

			var createResult = await store.CreateAsync(user, NoCancellationToken);

			var newClaims = new List<Claim>
			{
				new Claim("type2","value2"),
				new Claim("type3","value3")
			};
			await store.AddClaimsAsync(user, newClaims, NoCancellationToken);
			await store.UpdateAsync(user, NoCancellationToken);

			var elasticUser = await store.FindByIdAsync(user.Id, NoCancellationToken);

			Assert.Equal(createResult, IdentityResult.Success);
			Assert.Equal(elasticUser.Claims.Count, 3);

		}

		[Fact]
		public async Task ReplaceClaim_Test()
		{

			var store = new ElasticUserStore<ElasticUser, ElasticRole>(_nestClient, _index);

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
				Email = new ElasticConfirmation("test@test.com")
			};

			var createResult = await store.CreateAsync(user, NoCancellationToken);
			await store.ReplaceClaimAsync(user, new Claim("type2", "value2"), new Claim("type2", "value22"), NoCancellationToken);
			await store.UpdateAsync(user, NoCancellationToken);
			var elasticUser = await store.FindByIdAsync(user.Id, NoCancellationToken);
			var changedClaim = elasticUser.Claims.Find(x => x.Value == "value22");

			Assert.Equal(createResult, IdentityResult.Success);
			Assert.NotNull(changedClaim);

		}

		[Fact]
		public async Task RemoveClaim_Test()
		{

			var store = new ElasticUserStore<ElasticUser, ElasticRole>(_nestClient, _index);

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
				Email = new ElasticConfirmation("test@test.com")
			};

			var createResult = await store.CreateAsync(user, NoCancellationToken);

			var claimsToBeRemoved = new List<Claim>
			{
				new Claim("type2","value2")
			};

			await store.RemoveClaimsAsync(user, claimsToBeRemoved, NoCancellationToken);

			var elasticUser = await store.FindByIdAsync(user.Id, NoCancellationToken);

			Assert.Equal(createResult, IdentityResult.Success);
			Assert.Equal(elasticUser.Claims.Count, 2);

		}

		[Fact]
		public async Task GetUsersForClaim_Test()
		{

			var store = new ElasticUserStore<ElasticUser, ElasticRole>(_nestClient, _index);

			var user1 = new ElasticUser
			{
				UserName = "test1",
				PasswordHash = "phash",
				Logins = new List<ElasticUserLogin> { new ElasticUserLogin(new UserLoginInfo("prov1", "key1", "test1")) },
				Claims = new List<ElasticClaim> {
					new ElasticClaim("type", "value1")
				},
				Email = new ElasticConfirmation("test1@test.com")
			};
			var user2 = new ElasticUser
			{
				UserName = "test2",
				PasswordHash = "phash",
				Logins = new List<ElasticUserLogin> { new ElasticUserLogin(new UserLoginInfo("prov2", "key2", "test2")) },
				Claims = new List<ElasticClaim> {
					new ElasticClaim("type", "value2")
				},
				Email = new ElasticConfirmation("test2@test.com")
			};

			var createResult1 = await store.CreateAsync(user1, NoCancellationToken);
			var createResult2 = await store.CreateAsync(user2, NoCancellationToken);
			var elasticUser = await store.FindByLoginAsync("prov2", "key2", NoCancellationToken);

			var foundByClaim = await store.GetUsersForClaimAsync(new Claim("type", "value2"), NoCancellationToken);

			Assert.Equal(createResult1, IdentityResult.Success);
			Assert.Equal(createResult2, IdentityResult.Success);
			Assert.Equal(foundByClaim.Count, 1);

		}


		[Fact]
		public async Task AddToRole_Test()
		{

			var store = new ElasticUserStore<ElasticUser, ElasticRole>(_nestClient, _index);

			var user = new ElasticUser
			{
				UserName = "test1",
				PasswordHash = "phash",
				Logins = new List<ElasticUserLogin> { new ElasticUserLogin(new UserLoginInfo("prov1", "key1", "test1")) },
				Claims = new List<ElasticClaim> {
					new ElasticClaim("type", "value1")
				},
				Email = new ElasticConfirmation("test1@test.com")
			};

			var createResult = await store.CreateAsync(user, NoCancellationToken);

			await store.AddToRoleAsync(user, "role1", NoCancellationToken);

			var elasticUser = await store.FindByIdAsync(user.Id, NoCancellationToken);

			var isInRole = await store.IsInRoleAsync(elasticUser, "role1", NoCancellationToken);

			Assert.Equal(createResult, IdentityResult.Success);
			Assert.True(isInRole);

		}

		[Fact]
		public async Task GetUsersInRole_Test()
		{

			var store = new ElasticUserStore<ElasticUser, ElasticRole>(_nestClient, _index);

			var user1 = new ElasticUser
			{
				UserName = "test1",
				PasswordHash = "phash",
				Logins = new List<ElasticUserLogin> { new ElasticUserLogin(new UserLoginInfo("prov1", "key1", "test1")) },
				Claims = new List<ElasticClaim> {
					new ElasticClaim("type", "value1")
				},
				Email = new ElasticConfirmation("test1@test.com")
			};
			var user2 = new ElasticUser
			{
				UserName = "test2",
				PasswordHash = "phash",
				Logins = new List<ElasticUserLogin> { new ElasticUserLogin(new UserLoginInfo("prov2", "key2", "test2")) },
				Claims = new List<ElasticClaim> {
					new ElasticClaim("type", "value2")
				},
				Email = new ElasticConfirmation("test2@test.com")
			};

			var createResult1 = await store.CreateAsync(user1, NoCancellationToken);
			var createResult2 = await store.CreateAsync(user2, NoCancellationToken);

			await store.AddToRoleAsync(user1, "role1", NoCancellationToken);
			await store.AddToRoleAsync(user1, "role2", NoCancellationToken);
			await store.AddToRoleAsync(user2, "role1", NoCancellationToken);

			var usersForRole1 = await store.GetUsersInRoleAsync("role1", NoCancellationToken);
			var usersForRole2 = await store.GetUsersInRoleAsync("role2", NoCancellationToken);

			Assert.Equal(createResult1, IdentityResult.Success);
			Assert.Equal(createResult2, IdentityResult.Success);
			Assert.Equal(usersForRole1.Count, 2);
			Assert.Equal(usersForRole2.Count, 1);

		}

		[Fact]
		public async Task SetSecurityStamp_Test()
		{

			var store = new ElasticUserStore<ElasticUser, ElasticRole>(_nestClient, _index);

			var user = new ElasticUser
			{
				UserName = "test1",
				PasswordHash = "phash",
				Logins = new List<ElasticUserLogin> { new ElasticUserLogin(new UserLoginInfo("prov1", "key1", "test1")) },
				Claims = new List<ElasticClaim> {
					new ElasticClaim("type", "value1")
				},
				Email = new ElasticConfirmation("test1@test.com")
			};

			var createResult = await store.CreateAsync(user, NoCancellationToken);
			await store.SetSecurityStampAsync(user, "stamp", NoCancellationToken);
			await store.UpdateAsync(user, NoCancellationToken);
			var elasticUser = await store.FindByIdAsync(user.Id, NoCancellationToken);

			Assert.Equal(createResult, IdentityResult.Success);
			Assert.Equal(elasticUser.SecurityStamp, "stamp");

		}

		[Fact]
		public async Task SetTwoFactorEnabled_Test()
		{

			var store = new ElasticUserStore<ElasticUser, ElasticRole>(_nestClient, _index);

			var user = new ElasticUser
			{
				UserName = "test1",
				PasswordHash = "phash",
				Logins = new List<ElasticUserLogin> { new ElasticUserLogin(new UserLoginInfo("prov1", "key1", "test1")) },
				Claims = new List<ElasticClaim> {
					new ElasticClaim("type", "value1")
				},
				Email = new ElasticConfirmation("test1@test.com")
			};

			var createResult = await store.CreateAsync(user, NoCancellationToken);
			await store.SetTwoFactorEnabledAsync(user, true, NoCancellationToken);
			await store.UpdateAsync(user, NoCancellationToken);
			var elasticUser = await store.FindByIdAsync(user.Id, NoCancellationToken);

			Assert.Equal(createResult, IdentityResult.Success);
			Assert.True(elasticUser.IsTwoFactorEnabled);

		}

		[Fact]
		public async Task FindByEmail_Test()
		{

			var store = new ElasticUserStore<ElasticUser, ElasticRole>(_nestClient, _index);

			var user1 = new ElasticUser
			{
				UserName = "test1",
				PasswordHash = "phash",
				Logins = new List<ElasticUserLogin> { new ElasticUserLogin(new UserLoginInfo("prov1", "key1", "test1")) },
				Email = new ElasticConfirmation("test1@test.com")
			};
			var user2 = new ElasticUser
			{
				UserName = "test2",
				PasswordHash = "phash",
				Logins = new List<ElasticUserLogin> { new ElasticUserLogin(new UserLoginInfo("prov2", "key2", "test2")) },
				Email = new ElasticConfirmation("Test2@test.com")
			};

			var createResult1 = await store.CreateAsync(user1, NoCancellationToken);
			var createResult2 = await store.CreateAsync(user2, NoCancellationToken);
			var elasticUser = await store.FindByEmailAsync("tEst2@test.com", NoCancellationToken);

			Assert.Equal(createResult1, IdentityResult.Success);
			Assert.Equal(createResult2, IdentityResult.Success);
			Assert.Equal(user2.Id, elasticUser.Id);

		}




	}
}
