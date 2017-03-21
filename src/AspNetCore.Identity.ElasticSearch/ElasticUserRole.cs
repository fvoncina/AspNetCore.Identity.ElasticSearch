using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCore.Identity.ElasticSearch
{
	[ElasticsearchType(Name = "user_roles", IdProperty = "Id")]
	public class ElasticUserRole
	{

		public ElasticUserRole() { }

		public ElasticUserRole(string normalizedRoleName, string userId)
		{
			this.NormalizedRoleName = normalizedRoleName;
			this.UserId = userId;
		}

		[Keyword]
		public string Id { get; set; } = Guid.NewGuid().ToString();

		[Keyword]
		public string NormalizedRoleName { get; set; }

		[Keyword]
		public string UserId { get; set; }

		[Date]
		public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.Now;


	}
}
