using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.Identity.ElasticSearch
{
    public class ElasticEmail : ElasticBaseConfirmation
    {
        public ElasticEmail() { }

        public ElasticEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException(nameof(email));
            }
            Email = email;
        }

        [Keyword]
        public string Email { get; set; }

    }
}
