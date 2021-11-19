using System.Reflection;
using Rogero.Common.ExtensionMethods;

namespace Rogero.FluentMigrator.Poco.RelationalTypes;

public static class DotnetToSqlTypeConverter
{
    public static readonly Dictionary<Type, Func<SqlTypeAttributeBase>> DotnetSqlTypeAttributeMap = new()
    {
        { typeof(string), () => new StringTypeAttribute() },
        { typeof(short), () => new Int16TypeAttribute() },
        { typeof(int), () => new Int32TypeAttribute() },
        { typeof(long), () => new Int64TypeAttribute() },
        { typeof(bool), () => new BoolTypeAttribute() },
        { typeof(byte[]), () => new BinaryTypeAttribute(int.MaxValue) },
        { typeof(byte), () => new BinaryTypeAttribute(1) },
        { typeof(decimal), () => new DecimalTypeAttribute(38, 12) },
        { typeof(DateTime), () => new DateTime2TypeAttribute() },
        { typeof(DateTimeOffset), () => new DateTimeOffsetTypeAttribute() },
        { typeof(double), () => new DoubleTypeAttribute() },
        { typeof(Guid), () => new GuidTypeAttribute() },
        { typeof(float), () => new FloatTypeAttribute() },
    };

    public static SqlTypeAttributeBase Convert(Type propertyType)
    {
        var isNullableStruct = propertyType.IsNullableStruct();

        propertyType = isNullableStruct
            ? Nullable.GetUnderlyingType(propertyType)
            : propertyType;

        var hasNullableAttribute = propertyType.HasNullableAttribute();

        var allowNull = isNullableStruct || hasNullableAttribute;

        if (DotnetSqlTypeAttributeMap.TryGetValue(propertyType, out var sqlTypeAttributeFunc))
        {
            var sqlTypeAttribute = sqlTypeAttributeFunc();
            sqlTypeAttribute.AllowNull = allowNull;
            return sqlTypeAttribute;
        }

        if (propertyType.IsEnum)
        {
            return new StringTypeAttribute()
            {
                AllowNull = allowNull,
                Length    = 100,
            };
        }

        throw new NotImplementedException(
            $"No automatic conversion from dotnet type: {propertyType.Name} to SQL Type.");
    }

    public static SqlTypeAttributeBase Convert(PropertyInfo propertyInfo)
    {
        var isMemberNullable = propertyInfo.IsMemberNullable();
        var propertyType     = propertyInfo.PropertyType;
        var isNullableStruct = propertyType.IsNullableStruct();
        var allowNull        = isMemberNullable || isNullableStruct;
        
        propertyType = propertyType.UnwrapNullable();

        return GetSqlTypeAttribute(propertyType, allowNull);
    }

    private static SqlTypeAttributeBase GetSqlTypeAttribute(Type propertyType, bool allowNull)
    {
        var sqlTypeAttribute = GetSqlTypeAttribute(propertyType);
        sqlTypeAttribute.AllowNull = allowNull;
        return sqlTypeAttribute;
    }

    private static SqlTypeAttributeBase GetSqlTypeAttribute(Type propertyType)
    {
        if (DotnetSqlTypeAttributeMap.TryGetValue(propertyType, out var sqlTypeAttributeFunc))
        {
            var sqlTypeAttribute = sqlTypeAttributeFunc();
            return sqlTypeAttribute;
        }

        if (propertyType.IsEnum)
            return new StringTypeAttribute() { Length = 100 };

        throw new NotImplementedException(
            $"No automatic conversion from dotnet type: {propertyType.FullName} to SQL Type.");
    }
}