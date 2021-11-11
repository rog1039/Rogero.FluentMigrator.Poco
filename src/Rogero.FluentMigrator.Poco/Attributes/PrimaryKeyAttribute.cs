namespace Rogero.FluentMigrator.Poco.Attributes;

public class PrimaryKeyAttribute : Attribute
{
    public bool IsPrimaryKey { get; }

    public PrimaryKeyAttribute(bool isPrimaryKey = true)
    {
        IsPrimaryKey = isPrimaryKey;
    }
}