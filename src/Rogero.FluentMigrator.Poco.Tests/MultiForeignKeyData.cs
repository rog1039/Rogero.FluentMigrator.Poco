using System.Collections.Generic;
using System.Data;

namespace Rogero.FluentMigrator.Poco.Tests
{
    public class MultiForeignKeyData
    {
        public string        ForeignSchemaName  { get; set; }
        public string        ForeignTableName   { get; set; }
        public IList<string> ForeignColumnNames { get; set; }
        
        public string        PrimarySchemaName  { get; set; }
        public string        PrimaryTableName   { get; set; }
        public IList<string> PrimaryColumnNames { get; set; }

        public Rule CascadeRule { get; set; }

        public string GetForeignKeyName()
        {
            return NameHelper.GetForeignKeyName(
                ForeignTableName,
                ForeignColumnNames,
                PrimaryTableName,
                PrimaryColumnNames);
        }
    }
}