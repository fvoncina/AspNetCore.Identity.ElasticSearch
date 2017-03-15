using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.Identity.ElasticSearch
{
    public class DuplicateException : Exception
    {
        public DuplicateException(string message) : base(message) { }

    }
}
