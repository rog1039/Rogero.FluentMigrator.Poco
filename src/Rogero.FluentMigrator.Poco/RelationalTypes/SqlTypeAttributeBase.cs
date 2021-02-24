using System;

namespace Rogero.FluentMigrator.Poco.RelationalTypes
{
    public abstract class SqlTypeAttributeBase : Attribute
    {
        public abstract string ToSqlServerDefinition();
    }
}