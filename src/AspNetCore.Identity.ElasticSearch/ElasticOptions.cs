using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCore.Identity.ElasticSearch
{
	public class ElasticOptions
	{		

		public string Index { get; set; } = "es-identity";

		public string UsersType { get; set; } = "users";

		public string RolesType { get; set; } = "roles";

		public string UserRolesType { get; set; } = "user_roles";

		public int DefaultQuerySize { get; set; } = 1000;

		public int DefaultShards { get; set; } = 1;

		public int DefaultReplicas { get; set; } = 0;

	}
}
