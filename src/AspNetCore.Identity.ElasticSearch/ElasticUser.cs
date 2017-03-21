using Nest;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace AspNetCore.Identity.ElasticSearch
{
	[ElasticsearchType(Name = "users", IdProperty = "Id")]
	public class ElasticUser: ElasticBaseRecord
	{

		private string _userName = string.Empty;

		[Text]
		public string UserName
		{
			get { return _userName; }
			set
			{
				_userName = value;
				Normalized = _userName?.GenerateSlug();
			}
		}

		[Object]
		public ElasticConfirmation Email { get; set; }

		[Object]
		public ElasticConfirmation PhoneNumber { get; set; }

		[Keyword]
		public string PasswordHash { get; set; }

		[Keyword]
		public string SecurityStamp { get; set; }

		[Boolean]
		public bool IsTwoFactorEnabled { get; set; }

		[Nested(Enabled = true)]
		public List<ElasticUserLogin> Logins { get; set; } = new List<ElasticUserLogin>();

		[Number(NumberType.Integer)]
		public int AccessFailedCount { get; set; }

		[Boolean]
		public bool IsLockoutEnabled { get; set; }

		[Date]
		public DateTimeOffset? LockoutEndDate { get; set; }		

	}
}
