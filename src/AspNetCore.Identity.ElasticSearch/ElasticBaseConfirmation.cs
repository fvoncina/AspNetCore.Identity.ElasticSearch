using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.Identity.ElasticSearch
{
	public class ElasticConfirmation
	{

		public ElasticConfirmation() { }

		public ElasticConfirmation(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentException(nameof(value));
			}
			Value = value;
			Normalized = value.ToLowerInvariant();
		}

		[Date]
		public DateTimeOffset? ConfirmedOn { get; set; }

		[Boolean]
		public bool Confirmed { get; set; }

		[Keyword]
		public string Normalized { get; set; }

		[Text]
		public string Value { get; set; }


	}
}
