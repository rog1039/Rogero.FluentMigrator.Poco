using System;
using Rogero.FluentMigrator.Poco.RelationalTypes;

namespace Rogero.FluentMigrator.Poco
{
    public class ColumnData
    {
        public ColumnDataName      ColumnDataName { get; set; }
        
        public ColumnDataPrimaryKey?  PrimaryKeyInformation  { get; set; }
        public ColumnDataIdentity?    IdentityInformation    { get; set; }
        public ColumnDataForeignKey?  ForeignKeyInformation  { get; set; }
        public SqlTypeAttributeBase?  SqlTypeAttribute       { get; set; }
        public ColumnDataCascadeRule? CascadeRuleInformation { get; set; }

        public ColumnData(ColumnDataName columnDataName)
        {
            ColumnDataName = columnDataName;
        }

        public override string ToString()
        {
            
            var ident = IdentityInformation != null 
                ? $", Ident (Seed:{IdentityInformation.Seed}, Increment:{IdentityInformation.Increment})" 
                : String.Empty;
            var primaryKey = PrimaryKeyInformation.IsPrimaryKey
                ? $" | PK (true)" + ident
                : String.Empty;
            var foreignKey = ForeignKeyInformation != null
                ? $" | FK ({ForeignKeyInformation.ToString()})"
                : String.Empty;
            var type = SqlTypeAttribute.ToSqlServerDefinition();
            return
                $"{ColumnDataName.Name}: Type ({type})" +
                primaryKey +
                foreignKey;
        }
    }

    public record UniqueConstraintInformation(string Name);
}