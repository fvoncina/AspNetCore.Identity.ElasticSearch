using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCore.Identity.ElasticSearch
{
	public abstract class ElasticBaseRecord
	{

		[Keyword]
		public string Id { get; set; } = Guid.NewGuid().ToString();

		[Keyword]
		public string Normalized { get; set; }

		[Nested]
		public List<ElasticClaim> Claims { get; set; } = new List<ElasticClaim>();

		[Date]
		public DateTimeOffset? DeletedOn { get; set; }

		[Date]
		public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.Now;

		[Boolean]
		public bool Deleted { get; set; }

	}
}
