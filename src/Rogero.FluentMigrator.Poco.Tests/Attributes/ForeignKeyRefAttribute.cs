using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Rogero.FluentMigrator.Poco.Tests
{
    public class ForeignKeyRefAttribute : Attribute
    {
        public Type?  PrimaryType       { get; set; }
        public string PrimarySchemaName { get; }
        public string PrimaryTableName  { get; }
        public string PrimaryColumnName { get; }
        public string ForeignKeyGroupId { get; }
        public string ForeignKeyName    { get; }
        public Rule   CascadeRule       { get; }

        public ForeignKeyRefAttribute(Type   type,
                                      Rule   cascadeRule,
                                      string propertyName,
                                      string foreignKeyGroupId = null,
                                      string foreignKeyName    = null)
        {
            PrimaryType         = type;
            var (schema, table) = type.GetSchemaTableNames();

            PrimarySchemaName = schema;
            PrimaryTableName  = table;
            PrimaryColumnName = propertyName;
            CascadeRule       = cascadeRule;
            ForeignKeyGroupId = foreignKeyGroupId;
            ForeignKeyName    = foreignKeyName;
        }


        public ForeignKeyRefAttribute(string primarySchemaName,
                                      string primaryTableName,
                                      Rule   cascadeRule,
                                      string propertyName,
                                      string foreignKeyGroupId = null,
                                      string foreignKeyName    = null)
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