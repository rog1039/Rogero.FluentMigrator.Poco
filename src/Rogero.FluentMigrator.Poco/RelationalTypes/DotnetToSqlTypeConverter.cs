using System;
using System.Collections.Generic;

namespace Rogero.FluentMigrator.Poco.RelationalTypes
{
    public static class DotnetToSqlTypeConverter
    {
        private static readonly Dictionary<Type, SqlTypeAttributeBase> DotnetSqlTypeAttributeMap = new()
        {
            {typeof(string), new StringTypeAttribute()},
            {typeof(short), new Int16TypeAttribute()},
            {typeof(int), new Int32TypeAttribute()},
            {typeof(long), new Int64TypeAttribute()},
            {typeof(bool), new BoolTypeAttribute()},
            {typeof(byte[]), new BinaryTypeAttribute(int.MaxValue)},
            {typeof(byte), new BinaryTypeAttribute(1)},
            {typeof(decimal), new DecimalTypeAttribute(38, 12)},
            {typeof(DateTime), new DateTime2TypeAttribute()},
            {typeof(DateTimeOffset), new DateTimeOffsetTypeAttribute()},
            {typeof(double), new DoubleTypeAttribute()},
            {typeof(Guid), new GuidTypeAttribute()},
            {typeof(float), new FloatTypeAttribute()},
        };

        public static SqlTypeAttributeBase Convert(Type propertyType)
        {
            var isNullableStruct = propertyType.IsNullableStruct();
            
            propertyType = isNullableStruct
                ? Nullable.GetUnderlyingType(propertyType)
                : propertyType;

            var hasNullableAttribute = propertyType.HasNullableAttribute();
            
            if (DotnetSqlTypeAttributeMap.TryGetValue(propertyType, out var sqlTypeAttribute))
            {
                sqlTypeAttribute.AllowNull = (isNullableStruct || hasNullableAttribute);
                return sqlTypeAttribute;
            }

            if (propertyType.IsEnum) return new StringTypeAttribute();
            
            throw new NotImplementedException(
                $"No automatic conversion from dotnet type: {propertyType.Name} to SQL Type.");
        }
    }
}