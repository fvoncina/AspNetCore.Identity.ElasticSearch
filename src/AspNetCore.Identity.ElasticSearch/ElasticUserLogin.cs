using Microsoft.AspNetCore.Identity;
using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.Identity.ElasticSearch
{
    public class ElasticUserLogin : IEquatable<ElasticUserLogin>, IEquatable<UserLoginInfo>
    {

        public ElasticUserLogin() { }

        public ElasticUserLogin(string loginProvider, string providerKey, string providerDisplayName)
        {
            LoginProvider = loginProvider;
            ProviderKey = providerKey;
            ProviderDisplayName = providerDisplayName;
        }

        public ElasticUserLogin(UserLoginInfo loginInfo)
        {

            if (loginInfo == null)
            {
                throw new ArgumentNullException(nameof(loginInfo));
            }

            LoginProvider = loginInfo.LoginProvider;
            ProviderKey = loginInfo.ProviderKey;
            ProviderDisplayName = loginInfo.ProviderDisplayName;
        }

        [Keyword]
        public string LoginProvider { get; set; }

        [Keyword]
        public string ProviderKey { get; set; }

        [Keyword]
        public string ProviderDisplayName { get; set; }

        public bool Equals(ElasticUserLogin other)
        {
            return other.LoginProvider.Equals(LoginProvider)
                && other.ProviderKey.Equals(ProviderKey);
        }

        public bool Equals(UserLoginInfo other)
        {
            return other.LoginProvider.Equals(LoginProvider)
                && other.ProviderKey.Equals(ProviderKey);
        }

        public override bool Equals(object o)
        {
            return base.Equals(o);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((LoginProvider != null ? ProviderKey.GetHashCode() : 0) * 397) ^ (LoginProvider != null ? ProviderKey.GetHashCode() : 0);
            }
        }

        public static bool operator ==(ElasticUserLogin left, UserLoginInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ElasticUserLogin left, UserLoginInfo right)
        {
            return !Equals(left, right);
        }


    }
}
