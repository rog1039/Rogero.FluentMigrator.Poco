using System;
using System.Data.SqlTypes;
using System.Reflection;
using FluentMigrator.Infrastructure.Extensions;
using Rogero.FluentMigrator.Poco.Attributes;
using Rogero.FluentMigrator.Poco.RelationalTypes;

namespace Rogero.FluentMigrator.Poco
{
    public static class ColumnDataFactory
    {
        public static ColumnData GetInfo(TableData tableData, PropertyInfo propertyInfo)
        {
            var columnNameInformation = GetColumnName(tableData, propertyInfo);
            var columnData            = new ColumnData(columnNameInformation);

            columnData.SqlTypeAttribute       = GetSqlTypeAttribute(tableData, columnData, propertyInfo);
            columnData.PrimaryKeyInformation  = GetPrimaryKeyInfo(tableData, columnData, propertyInfo);
            columnData.IdentityInformation    = GetIdentityInfo(tableData, columnData, propertyInfo);
            columnData.ForeignKeyInformation  = GetForeignKeyInfo(tableData, propertyInfo);
            columnData.CascadeRuleInformation = GetCascadeRuleInfo(tableData, propertyInfo);

            return columnData;
        }

        public static ColumnDataName GetColumnName(PropertyInfo propertyInfo)
        {
            var columnNameAttribute = propertyInfo.GetOneAttribute<ColumnNameAttribute>();
            return columnNameAttribute != null
                ? new ColumnDataName(columnNameAttribute.ColumnName)
                : new ColumnDataName(propertyInfo.Name);
        }

        private static ColumnDataName GetColumnName(TableData    tableData,
                                                    PropertyInfo propertyInfo)
        {
            return GetColumnName(propertyInfo);
        }

        private static SqlTypeAttributeBase GetSqlTypeAttribute(TableData    tableData, ColumnData columnData,
                                                                PropertyInfo propertyInfo)
        {
            var sqlTypeAttribute = propertyInfo.GetAttributeAssignableTo<SqlTypeAttributeBase>();
            if (sqlTypeAttribute != null) return sqlTypeAttribute;

            var columnNameIsRowVersion = string.Equals(columnData.ColumnDataName.Name, "rowversion",
                                                  StringComparison.InvariantCultureIgnoreCase);
            
            var propertyNameIsRowVersion = string.Equals(propertyInfo.Name, "rowversion",
                                                  StringComparison.InvariantCultureIgnoreCase);

            if (columnNameIsRowVersion || propertyNameIsRowVersion) return new RowVersionType();

            return DotnetToSqlTypeConverter.Convert(propertyInfo.PropertyType);
        }

        private static ColumnDataPrimaryKey GetPrimaryKeyInfo(TableData    tableData,
                                                              ColumnData   columnData,
                                                              PropertyInfo propertyInfo)
        {
            var isNameId = string.Equals("Id", propertyInfo.Name, StringComparison.InvariantCultureIgnoreCase);
            if (isNameId) return new ColumnDataPrimaryKey(true);
            
            var primaryKeyAttribute = propertyInfo.GetOneAttribute<PrimaryKeyAttribute>();
            return primaryKeyAttribute != null
                ? new ColumnDataPrimaryKey(primaryKeyAttribute.IsPrimaryKey)
                : new ColumnDataPrimaryKey(false);
        }

        private static ColumnDataIdentity? GetIdentityInfo(TableData    tableData,
                                                           ColumnData   columnData,
                                                           PropertyInfo propertyInfo)
        {
            var identityAttribute = propertyInfo.GetOneAttribute<IdentityAttribute>();
            if (identityAttribute != null)
                return new ColumnDataIdentity(identityAttribute.Seed, identityAttribute.Increment);

            var isColumnPrimaryKey = columnData.PrimaryKeyInformation?.IsPrimaryKey == true;
            var isColumnAnInt = columnData.SqlTypeAttribute is Int16TypeAttribute
                             || columnData.SqlTypeAttribute is Int32TypeAttribute
                             || columnData.SqlTypeAttribute is Int64TypeAttribute;
            if (isColumnPrimaryKey && isColumnAnInt)
            {
                return new ColumnDataIdentity(1, 1);
            }
            
            return identityAttribute != null
                ? new ColumnDataIdentity(identityAttribute.Seed, identityAttribute.Increment)
                : null;
        }

        private static ColumnDataForeignKey? GetForeignKeyInfo(TableData    tableData,
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

            var fkInfo = new ColumnDataForeignKey(foreignSchema,
                                                  foreignTable,
                                                  foreignColumnName,
                                                  fkAttribute.PrimarySchemaName,
                                                  fkAttribute.PrimaryTableName,
                                                  primaryColumnName,
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

        private static ColumnDataCascadeRule? GetCascadeRuleInfo(TableData tableData, PropertyInfo propertyInfo)
        {
            var cascadeRuleAttribute = propertyInfo.GetOneAttribute<CascadeRuleAttribute>();
            return cascadeRuleAttribute != null
                ? new ColumnDataCascadeRule(cascadeRuleAttribute.CascadeRule)
                : null;

        }
    }
}