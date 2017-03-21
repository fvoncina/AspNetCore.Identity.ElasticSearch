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
	public class ElasticRoleStoreTests : BaseStoreTests
	{

		public ElasticRoleStoreTests() : base()
		{
		}

		[Fact]
		public async Task Create_Test()
		{

			var store = new ElasticRoleStore<ElasticUser, ElasticRole>(_nestClient, new ElasticOptions { Index=_index});
			var role = new ElasticRole
			{
				Id = "role1",
				Name = "Role 1",
				Claims = new List<ElasticClaim> {
					new ElasticClaim("type-a", "value1"),
					new ElasticClaim("type-b", "value2")
				}
			};
			var result = await store.CreateAsync(role, NoCancellationToken);			

			Assert.Equal(result, IdentityResult.Success);

		}

		[Fact]
		public async Task Delete_Test()
		{

			var store = new ElasticRoleStore<ElasticUser, ElasticRole>(_nestClient, new ElasticOptions { Index=_index});
			var role = new ElasticRole
			{
				Id = "role1",
				Name = "Role 1",
				Claims = new List<ElasticClaim> {
					new ElasticClaim("type-a", "value1"),
					new ElasticClaim("type-b", "value2")
				}
			};

			var createResult = await store.CreateAsync(role, NoCancellationToken);
			var deleteResult = await store.DeleteAsync(role, NoCancellationToken);

			Assert.Equal(createResult, IdentityResult.Success);
			Assert.Equal(deleteResult, IdentityResult.Success);

		}

		[Fact]
		public async Task FindById_Test()
		{

			var store = new ElasticRoleStore<ElasticUser, ElasticRole>(_nestClient, new ElasticOptions { Index=_index});
			var role1 = new ElasticRole
			{
				Id = "role1",
				Name = "Role 1",
				Claims = new List<ElasticClaim> {
					new ElasticClaim("type-a", "value1"),
					new ElasticClaim("type-b", "value1")
				}
			};
			var role2 = new ElasticRole
			{
				Id = "role2",
				Name = "Role 2",
				Claims = new List<ElasticClaim> {
					new ElasticClaim("type-a", "value2"),
					new ElasticClaim("type-b", "value2")
				}
			};

			var createResult1 = await store.CreateAsync(role1, NoCancellationToken);
			var createResult2 = await store.CreateAsync(role2, NoCancellationToken);

			var elasticRole = await store.FindByIdAsync(role1.Id, NoCancellationToken);

			Assert.Equal(createResult1, IdentityResult.Success);
			Assert.Equal(createResult2, IdentityResult.Success);
			Assert.Equal(role1.Id, elasticRole.Id);

		}		

		[Fact]
		public async Task FindByName_Test()
		{

			var store = new ElasticRoleStore<ElasticUser, ElasticRole>(_nestClient, new ElasticOptions { Index=_index});
			var role1 = new ElasticRole
			{
				Id = "role1",
				Name = "Role 1",
				Claims = new List<ElasticClaim> {
					new ElasticClaim("type-a", "value1"),
					new ElasticClaim("type-b", "value1")
				}
			};
			var role2 = new ElasticRole
			{
				Id = "role2",
				Name = "Role 2",
				Claims = new List<ElasticClaim> {
					new ElasticClaim("type-a", "value2"),
					new ElasticClaim("type-b", "value2")
				}
			};

			var createResult1 = await store.CreateAsync(role1, NoCancellationToken);
			var createResult2 = await store.CreateAsync(role2, NoCancellationToken);

			var elasticRole = await store.FindByNameAsync(role2.Name, NoCancellationToken);

			Assert.Equal(createResult1, IdentityResult.Success);
			Assert.Equal(createResult2, IdentityResult.Success);
			Assert.Equal(role2.Id, elasticRole.Id);

		}
		
	}
}
