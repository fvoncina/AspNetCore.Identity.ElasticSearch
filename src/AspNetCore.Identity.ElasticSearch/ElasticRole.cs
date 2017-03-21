using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCore.Identity.ElasticSearch
{
	[ElasticsearchType(Name = "roles", IdProperty = "Id")]
	public class ElasticRole : ElasticBaseRecord
	{

		private string _name = string.Empty;

		[Text]
		public string Name
		{
			get { return _name; }
			set
			{
				_name = value;
				Normalized = _name?.GenerateSlug();
			}
		}		

	}
}
