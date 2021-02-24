using System;
using Rogero.FluentMigrator.Poco.Tests.RelationalTypes;

namespace Rogero.FluentMigrator.Poco.Tests
{
    public class ColumnData
    {
        public ColumnNameInformation      ColumnNameInformation { get; set; }
        public ColumnType                 ColumnType            { get; set; }
        
        public PrimaryKeyInformation?     PrimaryKeyInformation { get; set; }
        public ColumnIdentityInformation? IdentityInformation   { get; set; }
        public ForeignKeyInformation?     ForeignKeyInformation { get; set; }
        public SqlTypeAttribute?          SqlTypeAttribute      { get; set; }         

        public ColumnData(ColumnNameInformation columnNameInformation, ColumnType columnType)
        {
            ColumnNameInformation = columnNameInformation;
            ColumnType            = columnType;
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
                $"{ColumnNameInformation.Name}: Type ({type})" +
                primaryKey +
                foreignKey;
        }
    }

    public record UniqueConstraintInformation(string Name);
}