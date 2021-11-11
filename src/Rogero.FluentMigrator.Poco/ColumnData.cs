using Rogero.FluentMigrator.Poco.RelationalTypes;

namespace Rogero.FluentMigrator.Poco;

public class ColumnData
{
    public ColumnDataName ColumnDataName { get; set; }
        
    public ColumnDataPrimaryKey?  PrimaryKeyInformation  { get; set; }
    public ColumnDataIdentity?    IdentityInformation    { get; set; }
    public ColumnDataForeignKey?  ForeignKeyInformation  { get; set; }
    public SqlTypeAttributeBase?  SqlTypeAttribute       { get; set; }
    public ColumnDataCascadeRule? CascadeRuleInformation { get; set; }
    public ColumnOrderInformation ColumnOrderInformation { get; set; }

    public ColumnData(ColumnDataName columnDataName)
    {
        ColumnDataName = columnDataName;
    }

    public override string ToString()
    {
            
        var ident = IdentityInformation != null 
            ? $", Ident (Seed:{IdentityInformation.Seed}, Increment:{IdentityInformation.Increment})" 
            : string.Empty;
        var primaryKey = PrimaryKeyInformation?.IsPrimaryKey == true
            ? $" | PK (true)" + ident
            : string.Empty;
        var foreignKey = ForeignKeyInformation != null
            ? $" | FK ({ForeignKeyInformation.ToString()})"
            : string.Empty;
        var type = SqlTypeAttribute.ToSqlServerDefinitionWithNullable();
        return
            $"{ColumnDataName.Name}: ({type})" +
            primaryKey +
            foreignKey;
    }
}

public record UniqueConstraintInformation(string Name);

public record ColumnOrderInformation(int Order);