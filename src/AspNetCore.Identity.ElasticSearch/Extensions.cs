using Nest;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace AspNetCore.Identity.ElasticSearch
{
    public static class Extensions
    {

        public static string NestedProperty<TRoot, TProperty>(this Inferrer inferrer, Expression<Func<TRoot, object>> rootExpression, Expression<Func<TProperty, object>> propExpression)
        {
            return $"{inferrer.PropertyName(rootExpression)}.{inferrer.PropertyName(propExpression)}";
        }

    }
}
