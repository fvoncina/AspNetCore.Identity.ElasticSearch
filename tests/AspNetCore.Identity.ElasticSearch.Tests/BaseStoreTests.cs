using Elasticsearch.Net;
using Nest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AspNetCore.Identity.ElasticSearch.Tests
{
	public abstract class BaseStoreTests : IDisposable
	{
		internal string _index = "test_identity_es";
		internal readonly IElasticClient _nestClient;
		internal readonly CancellationToken NoCancellationToken = CancellationToken.None;

		public BaseStoreTests()
		{
			_index += $"_{Guid.NewGuid().ToString()}";
			var connectionPool = new StaticConnectionPool(new[] { new Uri("http://localhost:9205") })
			{
				SniffedOnStartup = false
			};
			var esConnectionConfiguration = new ConnectionSettings(connectionPool);
			esConnectionConfiguration.DisableDirectStreaming(true);
			_nestClient = new ElasticClient(esConnectionConfiguration);			
		}

		public void Dispose()
		{
			_nestClient.DeleteIndex(_index);
		}
	}
}
