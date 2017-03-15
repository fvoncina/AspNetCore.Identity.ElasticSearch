using Elasticsearch.Net;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;
using AspNetCore.Identity.ElasticSearch;

namespace AspNetCore.Identity.ElasticSearch.Tests
{
    public class ExtensionsTests
    {
        [Fact]
        public void NestedPropertyInfer_Test()
        {            

            const string expected = "logins.loginProvider";
            var inferrer = new Nest.Inferrer(new ConnectionSettings());
            var actual = inferrer.NestedProperty<ElasticUser, ElasticUserLogin>(x => x.Logins, x => x.LoginProvider);
            Assert.Equal(expected, actual);

        }

    }

}
