namespace Rogero.FluentMigrator.Poco.RelationalTypes;

public abstract class SqlTypeAttributeBase : Attribute
{
    public          bool   AllowNull { get; set; }
    public abstract string ToSqlServerDefinition();

    public string ToSqlServerDefinitionWithNullable()
    {
        var nullablePart = AllowNull ? "NULL" : "NOT NULL";
        return ToSqlServerDefinition() + " " + nullablePart;
    }
        
    public override string ToString()
    {
        var nullablePart = AllowNull ? "  " : "* ";
        return nullablePart + ToSqlServerDefinition();
    }
}