using System.Data;

namespace Rogero.FluentMigrator.Poco.Attributes;

public class TableNameAttribute : Attribute
{
    public string TableName  { get; }
    public string SchemaName { get; }

    public TableNameAttribute(string tableName, string schemaName = "dbo")
    {
        TableName  = tableName;
        SchemaName = schemaName;
    }
}

public class CascadeRuleAttribute : Attribute
{
    public Rule CascadeRule { get; set; }

    public CascadeRuleAttribute(Rule cascadeRule)
    {
        CascadeRule = cascadeRule;
    }
}

public class IgnoreForMappingAttribute:Attribute{}