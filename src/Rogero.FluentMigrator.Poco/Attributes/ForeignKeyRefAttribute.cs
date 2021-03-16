using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Rogero.FluentMigrator.Poco.Attributes
{
    public class ForeignKeyRefAttribute : Attribute
    {
        public Type?   PrimaryType       { get; set; }
        public string  PrimarySchemaName { get; }
        public string  PrimaryTableName  { get; }
        public string  PrimaryColumnName { get; }
        public string? ForeignKeyGroupId { get; }
        public string? ForeignKeyName    { get; }
        public Rule    CascadeRule       { get; }

        public ForeignKeyRefAttribute(Type    type,
                                      Rule    cascadeRule,
                                      string? primaryColumn      = null,
                                      string? foreignKeyGroupId = null,
                                      string? foreignKeyName    = null)
        {
            PrimaryType         = type;
            var (schema, table) = type.GetSchemaTableNames();

            PrimarySchemaName = schema;
            PrimaryTableName  = table;
            CascadeRule       = cascadeRule;
            ForeignKeyGroupId = foreignKeyGroupId;
            ForeignKeyName    = foreignKeyName;

            PrimaryColumnName = string.IsNullOrWhiteSpace(primaryColumn)
                ? GetMatchingPrimaryKey()
                : primaryColumn;
        }

        public string GetMatchingPrimaryKey()
        {
            if(PrimaryType is null) return string.Empty;
            
            /*
             * This is turning out to be difficult. What is unfortunate is that we have no "good" way to get the primary
             * key information for this type. At this stage we don't have enough processed to be able to query the model
             * for the primary key(s) of this type. We can basically do an ad-hoc query here manually to come up with
             * the primary key. Unfortunately, we then have primary-key detection logic in two places, here, and also
             * in the column data factory that does primary key detection as well.
             */
             
            /*
             * Also, this method is trying to allow us to link two types together without specifying the primary key.
             * So, this really only works when there is a single primary key and that primary key has the same type
             * as this foreign key column.
             * So, we should be able to loop through all properties, select the Single that is a primary key, check the
             * data type against this column, and if we still have a match then that is our primary key we want to use.
             */
            var properties = PrimaryType.GetProperties()
                .Select(prop => new
                {
                    Property = prop,
                    PrimaryKeyInfo = ColumnDataFactory
                        .GetPrimaryKeyInfo(prop),
                    ColumnName = ColumnDataFactory.GetColumnName(prop)
                })
                .Where(z => z.PrimaryKeyInfo != null && z.PrimaryKeyInfo.IsPrimaryKey)
                .ToList();

            return properties.Count switch
            {
                0 => String.Empty,
                1 => properties[0].ColumnName.Name,
                _ => String.Empty
            };
        }


        public ForeignKeyRefAttribute(string  primarySchemaName,
                                      string  primaryTableName,
                                      Rule    cascadeRule,
                                      string  propertyName,
                                      string? foreignKeyGroupId = null,
                                      string? foreignKeyName    = null)
        {
            PrimarySchemaName = primarySchemaName;
            PrimaryTableName  = primaryTableName;
            PrimaryColumnName = propertyName;
            CascadeRule       = cascadeRule;
            ForeignKeyGroupId = foreignKeyGroupId;
            ForeignKeyName    = foreignKeyName;
        }

        public IList<string> GetColumnNames(Type type, IEnumerable<string> propertyNames)
        {
            var results        = new List<string>();
            var typeProperties = type.GetProperties();
            foreach (var propertyName in propertyNames)
            {
                var propertyInfo = typeProperties.Single(z => z.Name == propertyName);
                var columnName   = ColumnDataFactory.GetColumnName(propertyInfo);
                results.Add(columnName.Name);
            }

            return results;
        }
    }
}