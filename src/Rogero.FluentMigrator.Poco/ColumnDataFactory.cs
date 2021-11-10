using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentMigrator.Infrastructure.Extensions;
using Rogero.FluentMigrator.Poco.Attributes;
using Rogero.FluentMigrator.Poco.RelationalTypes;

namespace Rogero.FluentMigrator.Poco
{
    public static class ColumnDataFactory
    {
        public static ColumnData? GetInfo(TableData tableData, PropertyInfo propertyInfo)
        {
            var propertyAttributes = propertyInfo.GetCustomAttributes().ToList();

            if (propertyAttributes.Any(z => z.GetType() == typeof(IgnoreForMappingAttribute)))
            {
                return null;
            }

            var columnNameInformation = GetColumnName(propertyInfo);
            var columnData            = new ColumnData(columnNameInformation);

            columnData.SqlTypeAttribute       = GetSqlTypeAttribute(columnData, propertyInfo);
            columnData.PrimaryKeyInformation  = GetPrimaryKeyInfo(propertyInfo);
            columnData.IdentityInformation    = GetIdentityInfo(columnData, propertyInfo);
            columnData.ForeignKeyInformation  = GetForeignKeyInfo(tableData, propertyInfo);
            columnData.CascadeRuleInformation = GetCascadeRuleInfo(tableData, propertyInfo, propertyAttributes);
            columnData.ColumnOrderInformation = GetColumnOrderInfo(tableData, propertyInfo, propertyAttributes);

            return columnData;
        }

        private static ColumnOrderInformation? GetColumnOrderInfo(TableData tableData, PropertyInfo propertyInfo, List<Attribute> propertyAttributes)
        {
            var columnOrderAttrib = propertyAttributes.SingleOrDefaultOfType<ColumnOrderAttribute>();
            return columnOrderAttrib switch
            {
                null => new ColumnOrderInformation(0),
                _    => new ColumnOrderInformation(columnOrderAttrib.ColumnOrder)
            };
        }

        public static ColumnDataName GetColumnName(PropertyInfo propertyInfo)
        {
            var columnNameAttribute = propertyInfo.GetOneAttribute<ColumnNameAttribute>();
            return columnNameAttribute != null
                ? new ColumnDataName(columnNameAttribute.ColumnName)
                : new ColumnDataName(propertyInfo.Name);
        }

        private static SqlTypeAttributeBase GetSqlTypeAttribute(ColumnData   columnData,
                                                                PropertyInfo propertyInfo)
        {
            var sqlTypeAttribute = propertyInfo.GetCustomAttributes<SqlTypeAttributeBase>().SingleOrDefault();

            if (sqlTypeAttribute != null) return sqlTypeAttribute;

            var columnNameIsRowVersion = string.Equals(columnData.ColumnDataName.Name, "rowversion",
                                                       StringComparison.InvariantCultureIgnoreCase);

            var propertyNameIsRowVersion = string.Equals(propertyInfo.Name, "rowversion",
                                                         StringComparison.InvariantCultureIgnoreCase);

            if (columnNameIsRowVersion || propertyNameIsRowVersion) return new RowVersionTypeAttribute();

            // return DotnetToSqlTypeConverter.Convert(propertyInfo.PropertyType);
            return DotnetToSqlTypeConverter.Convert(propertyInfo);
        }

        public static ColumnDataPrimaryKey GetPrimaryKeyInfo(PropertyInfo propertyInfo)
        {
            var isNameId = string.Equals("Id", propertyInfo.Name, StringComparison.InvariantCultureIgnoreCase);
            if (isNameId) return new ColumnDataPrimaryKey(true);

            var primaryKeyAttribute = propertyInfo.GetOneAttribute<PrimaryKeyAttribute>();
            return primaryKeyAttribute != null
                ? new ColumnDataPrimaryKey(primaryKeyAttribute.IsPrimaryKey)
                : new ColumnDataPrimaryKey(false);
        }

        private static ColumnDataIdentity? GetIdentityInfo(ColumnData   columnData,
                                                           PropertyInfo propertyInfo)
        {
            var identityAttribute = propertyInfo.GetOneAttribute<IdentityAttribute>();
            if (identityAttribute != null)
                return new ColumnDataIdentity(identityAttribute.Seed, identityAttribute.Increment);

            /*
             * Now you must be: PK, Int16/32/64, & have name == Id;
             */
            var isColumnPrimaryKey = columnData.PrimaryKeyInformation?.IsPrimaryKey == true;
            var isColumnAnInt = columnData.SqlTypeAttribute is Int16TypeAttribute
                             || columnData.SqlTypeAttribute is Int32TypeAttribute
                             || columnData.SqlTypeAttribute is Int64TypeAttribute;
            var isColumnNamedId = string.Equals(columnData.ColumnDataName.Name,
                                                "id",
                                                StringComparison.OrdinalIgnoreCase);

            var shouldInferAsIdentityColumn = (isColumnNamedId && isColumnPrimaryKey && isColumnAnInt);
            return shouldInferAsIdentityColumn
                ? new ColumnDataIdentity(1, 1)
                : null;
        }

        private static ColumnDataForeignKey? GetForeignKeyInfo(TableData    tableData,
                                                               PropertyInfo propertyInfo)
        {
            var fkRefAttribute = propertyInfo.GetOneAttribute<ForeignKeyRefAttribute>();
            if (fkRefAttribute == null) return null;

            var primaryColumnRefType = GetPrimaryColumnRefType(fkRefAttribute);
            var primaryColumnName = GetPrimaryColumnName(primaryColumnRefType, fkRefAttribute, propertyInfo);

            var (foreignSchema, foreignTable) = tableData.TableName;
            var foreignColumnName = GetColumnName(propertyInfo).Name;

            var fkInfo = new ColumnDataForeignKey(foreignSchema,
                                                  foreignTable,
                                                  foreignColumnName,
                                                  fkRefAttribute.PrimarySchemaName,
                                                  fkRefAttribute.PrimaryTableName,
                                                  primaryColumnName,
                                                  fkRefAttribute.CascadeRule);
            
            fkInfo.GroupId = fkRefAttribute.ForeignKeyGroupId;
            return fkInfo;
        }

        private static string GetPrimaryColumnName(PrimaryColumnRefType   primaryColumnRefType,
                                                   ForeignKeyRefAttribute fkRefAttribute,
                                                   PropertyInfo           propertyInfo)
        {
            switch (primaryColumnRefType)
            {
                case PrimaryColumnRefType.Nothing:
                    /*
                     * In this case, our attribute is somthing like FKAttribute(CascadeRule.Cascade)
                     * We will take this to mean that the name of the column this attribute is attached to
                     * shares the same name as the primary key column.
                     */
                    return GetColumnName(propertyInfo).Name;
                
                case PrimaryColumnRefType.ColumnNameProvided:
                    return fkRefAttribute.PrimaryColumnName;
                
                case PrimaryColumnRefType.PropertyNameProvided when fkRefAttribute.PrimaryType is not null:
                    var property   = fkRefAttribute.PrimaryType.GetProperty(fkRefAttribute.PrimaryColumnName);
                    if (property is null)
                        throw new Exception(
                            $"No matching property found ({fkRefAttribute.PrimaryColumnName} on primary type: {fkRefAttribute.PrimaryType.FullName}");
                    var columnName = GetColumnName(property);
                    return columnName.Name;
                
                case PrimaryColumnRefType.PrimaryTypeOnlyProvided when fkRefAttribute.PrimaryType is not null:
                    var primaryKey = fkRefAttribute.GetMatchingPrimaryKey();
                    if (primaryKey.IsNullOrWhitespace())
                        throw new Exception(
                            $"Could not find a matching primary key on the primary type: {fkRefAttribute.PrimaryType.FullName}");
                    return primaryKey;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(primaryColumnRefType), primaryColumnRefType, null);
            }
        }

        public static PrimaryColumnRefType GetPrimaryColumnRefType(ForeignKeyRefAttribute fkRef)
        {
            var hasPrimaryType       = fkRef.PrimaryType != null;
            var hasPrimaryColumnText = fkRef.PrimaryColumnName.IsNotNullOrWhitespace();
            return (hasPrimaryType, hasPrimaryColumnText) switch
            {
                (false, false) => PrimaryColumnRefType.Nothing,
                (false, true)  => PrimaryColumnRefType.ColumnNameProvided,
                (true, false)  => PrimaryColumnRefType.PrimaryTypeOnlyProvided,
                (true, true)   => PrimaryColumnRefType.PropertyNameProvided,
            };
        }

        public enum PrimaryColumnRefType
        {
            Nothing,
            ColumnNameProvided,
            PropertyNameProvided,
            PrimaryTypeOnlyProvided,
        }

        private static ColumnDataCascadeRule? GetCascadeRuleInfo(TableData tableData, PropertyInfo propertyInfo,
                                                                 IEnumerable<Attribute> propertyAttributes)
        {
            var cascadeRuleAttribute = propertyInfo.GetOneAttribute<CascadeRuleAttribute>();
            return cascadeRuleAttribute != null
                ? new ColumnDataCascadeRule(cascadeRuleAttribute.CascadeRule)
                : null;
        }
    }
}