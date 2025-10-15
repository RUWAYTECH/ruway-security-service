using System;
using System.Linq.Expressions;

namespace SecurityMicroservice.Shared.Extensions;

// Add this helper somewhere in your project
public static class ExpressionExtensions
{
    public static Expression<Func<T, bool>> AndAlso<T>(
        this Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
    {
        if (expr1 == null) return expr2;
        if (expr2 == null) return expr1;

        var parameter = Expression.Parameter(typeof(T));
        var body = Expression.AndAlso(
            Expression.Invoke(expr1, parameter),
            Expression.Invoke(expr2, parameter)
        );
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}
