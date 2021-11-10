using System;
using System.Collections.Generic;
using System.Reflection;
using Rogero.Common.ExtensionMethods;

namespace Rogero.FluentMigrator.Poco.RelationalTypes
{
    public static class DotnetToSqlTypeConverter
    {
        public static readonly Dictionary<Type, Func<SqlTypeAttributeBase>> DotnetSqlTypeAttributeMap = new()
        {
            {typeof(string),()=> new StringTypeAttribute()},
            {typeof(short), ()=> new Int16TypeAttribute()},
            {typeof(int), ()=> new Int32TypeAttribute()},
            {typeof(long), ()=> new Int64TypeAttribute()},
            {typeof(bool), ()=> new BoolTypeAttribute()},
            {typeof(byte[]), ()=> new BinaryTypeAttribute(int.MaxValue)},
            {typeof(byte), ()=> new BinaryTypeAttribute(1)},
            {typeof(decimal), ()=> new DecimalTypeAttribute(38, 12)},
            {typeof(DateTime), ()=> new DateTime2TypeAttribute()},
            {typeof(DateTimeOffset),()=>  new DateTimeOffsetTypeAttribute()},
            {typeof(double), ()=> new DoubleTypeAttribute()},
            {typeof(Guid), ()=> new GuidTypeAttribute()},
            {typeof(float), ()=> new FloatTypeAttribute()},
        };

        public static SqlTypeAttributeBase Convert(Type propertyType)
        {
            var isNullableStruct = propertyType.IsNullableStruct();
            
            propertyType = isNullableStruct
                ? Nullable.GetUnderlyingType(propertyType)
                : propertyType;

            var hasNullableAttribute = propertyType.HasNullableAttribute();
            
            if (DotnetSqlTypeAttributeMap.TryGetValue(propertyType, out var sqlTypeAttributeFunc))
            {
                var sqlTypeAttribute = sqlTypeAttributeFunc();
                sqlTypeAttribute.AllowNull = (isNullableStruct || hasNullableAttribute);
                return sqlTypeAttribute;
            }

            if (propertyType.IsEnum)
            {
                return new StringTypeAttribute()
                {
                    AllowNull = isNullableStruct || hasNullableAttribute
                };
            }
            
            throw new NotImplementedException(
                $"No automatic conversion from dotnet type: {propertyType.Name} to SQL Type.");
        }

        public static SqlTypeAttributeBase Convert(PropertyInfo propertyInfo)
        {
            var isMemberNullable = propertyInfo.IsMemberNullable();
            var isNullableStruct = propertyInfo.PropertyType.IsNullableStruct();

            var propertyType = isNullableStruct
                ? Nullable.GetUnderlyingType(propertyInfo.PropertyType)
                : propertyInfo.PropertyType;

            if (DotnetSqlTypeAttributeMap.TryGetValue(propertyType, out var sqlTypeAttributeFunc))
            {
                var sqlTypeAttribute = sqlTypeAttributeFunc();
                sqlTypeAttribute.AllowNull = isMemberNullable;
                return sqlTypeAttribute;
            }
            
            if (propertyType.IsEnum)
            {
                return new StringTypeAttribute()
                {
                    AllowNull = isNullableStruct || isMemberNullable
                };
            }
            

            throw new NotImplementedException(
                $"No automatic conversion from dotnet type: {propertyType.FullName} to SQL Type.");
        }
    }
}