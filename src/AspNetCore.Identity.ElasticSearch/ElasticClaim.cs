using Nest;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace AspNetCore.Identity.ElasticSearch
{
    public class ElasticClaim : IEquatable<ElasticClaim>, IEquatable<Claim>
    {

        public ElasticClaim() { }

        public ElasticClaim(Claim claim)
        {
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            ClaimType = claim.Type;
            ClaimValue = claim.Value;
        }

        public ElasticClaim(string claimType, string claimValue)
        {
            if (string.IsNullOrEmpty(claimType))
            {
                throw new ArgumentNullException(nameof(claimType));
            }
            if (string.IsNullOrEmpty(claimValue))
            {
                throw new ArgumentNullException(nameof(claimValue));
            }

            ClaimType = claimType;
            ClaimValue = claimValue;
        }

        [Keyword]
        public string ClaimType { get; set; }

        [Keyword]
        public string ClaimValue { get; set; }

        public Claim AsClaim()
        {
            return new Claim(ClaimType, ClaimValue);
        }

        public bool Equals(ElasticClaim other)
        {
            return other.ClaimType.Equals(ClaimType)
                && other.ClaimValue.Equals(ClaimValue);
        }

        public bool Equals(Claim other)
        {
            return other.Type.Equals(ClaimType)
                && other.Value.Equals(ClaimValue);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override string ToString()
        {
            return ClaimType + ": " + ClaimValue;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((ClaimValue != null ? ClaimValue.GetHashCode() : 0) * 397) ^ (ClaimType != null ? ClaimType.GetHashCode() : 0);
            }
        }

        public static bool operator ==(ElasticClaim left, Claim right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ElasticClaim left, Claim right)
        {
            return !Equals(left, right);
        }


    }
}
