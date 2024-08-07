using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

public static class LambdaConverter
{
    public static Expression<Func<T, bool>> ConvertToLambda<T>(Dictionary<string, object> dictionary)
    {
        if (dictionary == null || dictionary.Count == 0)
        {
            throw new ArgumentNullException(nameof(dictionary), "Dictionary cannot be null or empty");
        }

        ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
        List<Expression> expressions = new List<Expression>();

        foreach (var kvp in dictionary)
        {
            PropertyInfo property = typeof(T).GetProperty(kvp.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property == null)
            {
                throw new ArgumentException($"Property '{kvp.Key}' not found on type '{typeof(T).Name}'");
            }

            object value = kvp.Value;

            MemberExpression propertyExpression = Expression.Property(parameter, property.Name);

            ConstantExpression valueExpression = Expression.Constant(value, property.PropertyType);

            // Create the equality expression
            BinaryExpression equalExpression = Expression.Equal(propertyExpression, valueExpression);

            expressions.Add(equalExpression);
        }

        // Combine all conditions with AND
        Expression finalExpression = expressions[0];
        for (int i = 1; i < expressions.Count; i++)
        {
            finalExpression = Expression.AndAlso(finalExpression, expressions[i]);
        }

        // Create and return the lambda expression
        return Expression.Lambda<Func<T, bool>>(finalExpression, parameter);
    }
}
