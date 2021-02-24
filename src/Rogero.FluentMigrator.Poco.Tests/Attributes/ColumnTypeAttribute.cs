using System;

namespace Rogero.FluentMigrator.Poco.Tests
{
}


namespace Rogero.FluentMigrator.Poco.Tests.RelationalTypes
{
    public abstract class SqlTypeAttribute : Attribute
    {
        public abstract string ToSqlServerDefinition();
    }

    /// <summary>
    /// By default, this specifies a variable, Unicode, max-length string.
    /// </summary>
    public class StringTypeAttribute : SqlTypeAttribute
    {
        public int    Length        { get; set; } = Int32.MaxValue;
        public object Collation     { get; set; }
        public bool   IsFixedLength { get; set; } = false;

        /// <summary>
        /// ANSI is an 8-bit character type extending from 7-bit ASCII.
        /// If this is false, then likely Unicode will be the encoding of this string.
        /// </summary>
        public bool IsAnsi { get; set; } = false;

        public StringTypeAttribute() { }

        public StringTypeAttribute(int length, bool isFixedLength = false, bool isAnsi = false)
        {
            Length        = length;
            IsFixedLength = isFixedLength;
            IsAnsi        = isAnsi;
        }

        public override string ToSqlServerDefinition()
        {
            return (IsAnsi, IsFixedLength) switch
            {
                (true, true)   => $"char({Length})",
                (true, false)  => $"nchar({Length})",
                (false, true)  => $"varchar({Length})",
                (false, false) => $"nvarchar({Length})",
            };
        }
    }

    public class Int32TypeAttribute : SqlTypeAttribute
    {
        public override string ToSqlServerDefinition() => "int";
    }

    public class Int16TypeAttribute : SqlTypeAttribute
    {
        public override string ToSqlServerDefinition() => "smallint";
    }

    public class Int64TypeAttribute : SqlTypeAttribute
    {
        public override string ToSqlServerDefinition() => "bigint";
    }

    public class BoolTypeAttribute : SqlTypeAttribute
    {
        public override string ToSqlServerDefinition() => "bit";
    }

    public class BinaryTypeAttribute : SqlTypeAttribute
    {
        public int  Length     { get; set; }
        public bool IsVariable { get; set; }

        public BinaryTypeAttribute(int length, bool isVariable = true)
        {
            Length     = length;
            IsVariable = isVariable || Length > 8000;
        }

        public override string ToSqlServerDefinition()
        {
            return IsVariable
                ? $"varbinary({Length})"
                : $"binary({Length})";
        }
    }

    public class ByteTypeAttribute : SqlTypeAttribute
    {
        public override string ToSqlServerDefinition() => "tinyint";
    }

    public class CurrencyTypeAttribute : SqlTypeAttribute
    {
        public override string ToSqlServerDefinition() => "money";
    }

    public class DateTime2TypeAttribute : SqlTypeAttribute
    {
        public override string ToSqlServerDefinition() => "datetime2(7)";
    }

    public class DateTimeOffsetTypeAttribute : SqlTypeAttribute
    {
        public int Precision { get; set; }

        public override string ToSqlServerDefinition() => "datetimeoffset";
    }

    public class DecimalTypeAttribute : SqlTypeAttribute
    {
        public int Precision { get; set; }
        public int Scale     { get; set; }

        public DecimalTypeAttribute(int precision, int scale)
        {
            Precision = precision;
            Scale     = scale;
        }

        public override string ToSqlServerDefinition() => $"decimal({Precision}, {Scale})";
    }

    public class DoubleTypeAttribute : SqlTypeAttribute
    {
        public override string ToSqlServerDefinition() => $"double";
    }

    public class GuidTypeAttribute : SqlTypeAttribute
    {
        public override string ToSqlServerDefinition() => $"uniqueidentifier";
    }

    public class FloatTypeAttribute : SqlTypeAttribute
    {
        public override string ToSqlServerDefinition() => $"float";
    }

    public class TimeTypeAttribute : SqlTypeAttribute
    {
        public override string ToSqlServerDefinition() => $"time";
    }

    public class XmlTypeAttribute : SqlTypeAttribute
    {
        public int    Length                  { get; set; }
        public override string ToSqlServerDefinition() => $"xml";
    }

    public class RowVersionType : SqlTypeAttribute
    {
        public override string ToSqlServerDefinition() => $"rowversion";
    }

    public class CustomTypeAttribute : SqlTypeAttribute
    {
        public string CustomName { get; set; }

        public override string ToSqlServerDefinition() => CustomName;
    }
}