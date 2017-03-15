using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.Identity.ElasticSearch
{
    public abstract class ElasticBaseConfirmation
    {
        [Date]
        public DateTime? ConfirmatedOn { get; set; }

        [Boolean]
        public bool Confirmed { get; set; }

    }
}
