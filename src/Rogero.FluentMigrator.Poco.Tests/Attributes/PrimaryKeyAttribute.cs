using System;

namespace Rogero.FluentMigrator.Poco.Tests
{
    public class PrimaryKeyAttribute : Attribute
    {
        public bool IsPrimaryKey { get; }

        public PrimaryKeyAttribute(bool isPrimaryKey = true)
        {
            IsPrimaryKey = isPrimaryKey;
        }
    }
}