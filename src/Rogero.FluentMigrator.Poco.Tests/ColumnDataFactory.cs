using System;
using System.Collections.Generic;
using System.Reflection;
using FluentMigrator.Infrastructure.Extensions;
using Rogero.FluentMigrator.Poco.Tests.RelationalTypes;

namespace Rogero.FluentMigrator.Poco.Tests
{
    public static class ColumnDataFactory
    {
        public static ColumnData GetInfo(TableData tableData, PropertyInfo propertyInfo)
        {
            var columnNameInformation = GetColumnName(tableData, propertyInfo);
            // var columnTypeInfo        = GetColumnType(tableData, propertyInfo);
            var sqlTypeInfo           = GetSqlType(tableData, propertyInfo);
            
            var primaryKeyInformation = GetPrimaryKeyInfo(tableData, propertyInfo);
            var identityInformation   = GetIdentityInfo(tableData, propertyInfo);
            var foreignKeyInformation = GetForeignKeyInfo(tableData, propertyInfo);

            return new ColumnData(columnNameInformation, null)
            {
                PrimaryKeyInformation = primaryKeyInformation,
                IdentityInformation   = identityInformation,
                ForeignKeyInformation = foreignKeyInformation,
                SqlTypeAttribute = sqlTypeInfo
            };
        }

        private static SqlTypeAttribute GetSqlType(TableData tableData, PropertyInfo propertyInfo)
        {
            var typeAttribute = propertyInfo.GetAttributeAssignableTo<SqlTypeAttribute>();
            if (typeAttribute != null)
            {
                return typeAttribute;
            }

            return ConvertDotnetTypeToSqlTypeAttribute(propertyInfo.PropertyType);
        }

        private static Dictionary<Type, SqlTypeAttribute> DotnetSqlTypeAttributeMap = new()
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

        private static Dictionary<Type, string> DotnetSqlTypeMap = new()
        {
            {typeof(DateTime), "datetime2"},
            {typeof(int), "int"},
            {typeof(long), "bigint"},
            {typeof(decimal), "decimal(38,12)"},
            {typeof(double), "float"},
            {typeof(string), "nvarchar(max)"},
            {typeof(bool), "bit"},
        };

        private static SqlTypeAttribute ConvertDotnetTypeToSqlTypeAttribute(Type propertyType)
        {
            if (DotnetSqlTypeAttributeMap.TryGetValue(propertyType, out var sqlTypeAttribute))
            {
                return sqlTypeAttribute;
            }

            throw new NotImplementedException(
                $"No automatic conversion from dotnet type: {propertyType.Name} to SQL Type.");
        }

        private static ColumnType ConvertDotnetTypeToSqlType(Type propertyType)
        {
            if (DotnetSqlTypeMap.TryGetValue(propertyType, out var sqlTypeString))
            {
                return new ColumnTypeCustom(sqlTypeString);
            }

            throw new NotImplementedException(
                $"No automatic conversion from dotnet type: {propertyType.Name} to SQL Type.");
        }

        private static ForeignKeyInformation? GetForeignKeyInfo(TableData    tableData,
                                                                PropertyInfo propertyInfo)
        {
            var fkAttribute = propertyInfo.GetOneAttribute<ForeignKeyRefAttribute>();
            if (fkAttribute == null) return null;

            /*
             * Start with unprocessed Primary column name.
             */
            var primaryColumnName = fkAttribute.PrimaryColumnName;
            if (fkAttribute.PrimaryType != null)
            {
                //If we have a primary type specified, then look at the property and extract the proper column name
                //if it is present.
                var property   = fkAttribute.PrimaryType.GetProperty(primaryColumnName);
                var columnName = GetColumnName(propertyInfo);
                primaryColumnName = columnName.Name;
            }

            var (foreignSchema, foreignTable) = tableData.TableName;
            var foreignColumnName = GetColumnName(propertyInfo).Name;

            var fkInfo = new ForeignKeyInformation(foreignSchema,
                                                   foreignTable,
                                                   new List<string>() {foreignColumnName},
                                                   fkAttribute.PrimarySchemaName,
                                                   fkAttribute.PrimaryTableName,
                                                   new List<string>() {primaryColumnName},
                                                   fkAttribute.CascadeRule);

            //Single column foreign key.
            if (fkAttribute.ForeignKeyGroupId.IsNullOrWhitespace())
            {
                return fkInfo;
            }
            else
            {
                /*
                 * This fk attribute is a part of a group which means this attribute only holds part of the data. We need to
                 * pass this fk attribute up to the parent object and let it combine the other fk attributes as necessary together
                 * to determine the final fk configuration.
                 */
                fkInfo.GroupId = fkAttribute.ForeignKeyGroupId;
                tableData.AddForeignKeyPart(fkInfo);
                //We are returning null since this property/column alone does not provide all the information and we do not
                //wish to apply a foreign key configuration from this info alone. Need the other parts from other column/props.
                return null;
            }
        }

        private static ColumnIdentityInformation? GetIdentityInfo(TableData    tableData,
                                                                  PropertyInfo propertyInfo)
        {
            var identityAttribute = propertyInfo.GetOneAttribute<IdentityAttribute>();
            return identityAttribute != null
                ? new ColumnIdentityInformation(identityAttribute.Seed, identityAttribute.Increment)
                : null;
        }

        private static PrimaryKeyInformation GetPrimaryKeyInfo(TableData    tableData,
                                                               PropertyInfo propertyInfo)
        {
            var primaryKeyAttribute = propertyInfo.GetOneAttribute<PrimaryKeyAttribute>();
            return primaryKeyAttribute != null
                ? new PrimaryKeyInformation(primaryKeyAttribute.IsPrimaryKey)
                : new PrimaryKeyInformation(propertyInfo.Name == "Id");
        }

        private static ColumnNameInformation GetColumnName(TableData    tableData,
                                                           PropertyInfo propertyInfo)
        {
            return GetColumnName(propertyInfo);
        }

        public static ColumnNameInformation GetColumnName(PropertyInfo propertyInfo)
        {
            var columnNameAttribute = propertyInfo.GetOneAttribute<ColumnNameAttribute>();
            return columnNameAttribute != null
                ? new ColumnNameInformation(columnNameAttribute.ColumnName)
                : new ColumnNameInformation(propertyInfo.Name);
        }
    }
}