using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.Identity.ElasticSearch
{
    [ElasticsearchType(Name = "users", IdProperty = "Id")]
    public class ElasticUser
    {

        private string _userName = string.Empty;

        [Keyword]
        public string Id { get; set; }

        [Keyword]
        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value?.ToLowerInvariant();
                NormalizedUserName = _userName;
            }
        }

        [Keyword]
        public string NormalizedUserName { get; set; }

        [Nested]
        public ElasticEmail Email { get; set; }

        [Nested]
        public ElasticPhoneNumber PhoneNumber { get; set; }

        [Keyword]
        public string PasswordHash { get; set; }

        [Keyword]
        public string SecurityStamp { get; set; }

        [Boolean]
        public bool IsTwoFactorEnabled { get; set; }

        [Nested(Enabled =true)]
        public List<ElasticClaim> Claims { get; set; } = new List<ElasticClaim>();

        [Nested(Enabled = true)]
        public List<ElasticUserLogin> Logins { get; set; } = new List<ElasticUserLogin>();

        [Number(NumberType.Integer)]
        public int AccessFailedCount { get; set; }

        [Boolean]
        public bool IsLockoutEnabled { get; set; }

        [Date]
        public DateTime? LockoutEndDate { get; set; }

        [Date]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        [Date]
        public DateTime? DeletedOn { get; set; }

        [Keyword]
        public List<string> Roles { get; set; } = new List<string>();

    }
}
