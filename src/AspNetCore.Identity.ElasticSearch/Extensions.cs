using Nest;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace AspNetCore.Identity.ElasticSearch
{
	public static class Extensions
	{

		public static string GenerateSlug(this string phrase)
		{
			string str = phrase.RemoveAccent().ToLower();
			// invalid chars           
			str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
			// convert multiple spaces into one space   
			str = Regex.Replace(str, @"\s+", " ").Trim();
			// cut and trim 
			str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
			str = Regex.Replace(str, @"\s", "-"); // hyphens   
			return str;
		}

		public static string RemoveAccent(this string text)
		{
			StringBuilder sbReturn = new StringBuilder();
			var arrayText = text.Normalize(NormalizationForm.FormD).ToCharArray();
			foreach (char letter in arrayText)
			{
				if (CharUnicodeInfo.GetUnicodeCategory(letter) != UnicodeCategory.NonSpacingMark)
					sbReturn.Append(letter);
			}
			return sbReturn.ToString();
		}

		public static string NestedProperty<TRoot, TProperty>(this Inferrer inferrer, Expression<Func<TRoot, object>> rootExpression, Expression<Func<TProperty, object>> propExpression)
		{
			return $"{inferrer.PropertyName(rootExpression)}.{inferrer.PropertyName(propExpression)}";
		}

		public static void AddClaim(this List<ElasticClaim> claims, Claim claim)
		{
			claims.AddClaim(claim.Type, claim.Value);
		}

		public static void AddClaim(this List<ElasticClaim> claims, ElasticClaim claim)
		{
			claims.AddClaim(claim.Type, claim.Value);
		}

		public static void AddClaim(this List<ElasticClaim> claims, string type, string value)
		{

			if (!claims.Any(c => c.Type == type && c.Value == value))
			{
				claims.Add(new ElasticClaim(type, value));
			}

		}

		public static IList<Claim> ToClaims(this List<ElasticClaim> claims)
		{
			return claims.Select(x => x.AsClaim()).ToList();
		}

		public static void RemoveClaim(this List<ElasticClaim> claims, Claim claim)
		{
			claims.RemoveClaim(claim.Type, claim.Value);
		}

		public static void RemoveClaim(this List<ElasticClaim> claims, string type)
		{
			claims.RemoveAll(x => x.Type == type);
		}

		public static void RemoveClaim(this List<ElasticClaim> claims, string type, string value)
		{
			claims.RemoveAll(x => x.Type == type && x.Value == value);
		}

	}
}
