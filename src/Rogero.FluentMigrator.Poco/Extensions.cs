using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentMigrator.Infrastructure.Extensions;
using Rogero.FluentMigrator.Poco.Attributes;

namespace Rogero.FluentMigrator.Poco
{
    public static class Extensions
    {
        public static T? GetAttributeAssignableTo<T>(this PropertyInfo propertyInfo) where T : Attribute
        {
            var attributes = propertyInfo.GetCustomAttributes();
            
            return (from attribute in attributes
                    let attributeType = attribute.GetType()
                    where typeof(T).IsAssignableFrom(attributeType)
                    select (T) attribute).FirstOrDefault();
        }
        
        public static bool IsNotNullOrWhitespace(this string s) => !IsNullOrWhitespace(s);

        public static bool IsNullOrWhitespace(this string s)
        {
            return string.IsNullOrWhiteSpace(s);
        }

        public static string StringJoin<T>(this IEnumerable<T> items, string separator)
        {
            return string.Join(separator, items);
        }

        public static T GetAttributeSingleOrDefault<T>(this Type type, bool inheritBaseAttributes = false)
        {
            var attribute = type
                .GetCustomAttributes(inheritBaseAttributes)
                .SingleOrDefault(z => z.GetType() == typeof(T));
            return (T) attribute;
        }

        public static T GetAttributeSingle<T>(this Type type, bool inheritBaseAttributes = false)
        {
            var attribute = type
                .GetCustomAttributes(inheritBaseAttributes)
                .Single(z => z.GetType() == typeof(T));
            return (T) attribute;
        }

        public static object GetAttributeSingle(this Type type, string attributeName,
                                                bool      inheritBaseAttributes = false)
        {
            var attribute = type
                .GetCustomAttributes(inheritBaseAttributes)
                .Single(z => z.GetType().Name == attributeName);
            return attribute;
        }

        public static object GetAttributeSingleOrDefault(this Type type, string attributeName,
                                                         bool      inheritBaseAttributes = false)
        {
            var attribute = type
                .GetCustomAttributes(inheritBaseAttributes)
                .SingleOrDefault(z => z.GetType().Name == attributeName);
            return attribute;
        }


        public static bool IsNullable(this Type type)
        {
            var isNullable = Nullable.GetUnderlyingType(type) != null;
            if (isNullable) return true;

            //Check nullable attribute: https://github.com/dotnet/roslyn/blob/master/docs/features/nullable-metadata.md
            var attribute = type.GetAttributeSingleOrDefault("NullableAttribute", true);
            if (attribute != null) return true;

            return false;
        }

        public static SchemaTableNames GetSchemaTableNames(this Type type)
        {
            var nameAttribute = type.GetOneAttribute<TableNameAttribute>();
            return nameAttribute != null
                ? new SchemaTableNames(nameAttribute.SchemaName, nameAttribute.TableName)
                : new SchemaTableNames("dbo",                    type.Name);
        }
    }
}