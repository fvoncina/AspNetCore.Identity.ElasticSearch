using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.Identity.ElasticSearch
{
    public class ElasticPhoneNumber : ElasticBaseConfirmation
    {
        [Keyword]
        public string Number { get; set; }
    }
}
