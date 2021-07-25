using System;

namespace Rogero.FluentMigrator.Poco.RelationalTypes
{
    /// <summary>
    /// By default, this specifies a variable, Unicode, max-length string.
    /// </summary>
    public class StringTypeAttribute : SqlTypeAttributeBase
    {
        public int     Length        { get; set; } = Int32.MaxValue;
        public object? Collation     { get; set; }
        public bool    IsFixedLength { get; set; } = false;

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
                (true, true)   => $"char({Length.ToSqlLength()})",
                (true, false)  => $"nchar({Length.ToSqlLength()})",
                (false, true)  => $"varchar({Length.ToSqlLength()})",
                (false, false) => $"nvarchar({Length.ToSqlLength()})",
            };
        }

    }

    public class Int32TypeAttribute : SqlTypeAttributeBase
    {
        public override string ToSqlServerDefinition() => "int";
    }

    public class Int16TypeAttribute : SqlTypeAttributeBase
    {
        public override string ToSqlServerDefinition() => "smallint";
    }

    public class Int64TypeAttribute : SqlTypeAttributeBase
    {
        public override string ToSqlServerDefinition() => "bigint";
    }

    public class BoolTypeAttribute : SqlTypeAttributeBase
    {
        public override string ToSqlServerDefinition() => "bit";
    }

    public class BinaryTypeAttribute : SqlTypeAttributeBase
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
                ? $"varbinary({Length.ToSqlLength()})"
                : $"binary({Length.ToSqlLength()})";
        }
    }

    public class ByteTypeAttribute : SqlTypeAttributeBase
    {
        public override string ToSqlServerDefinition() => "tinyint";
    }

    public class CurrencyTypeAttribute : SqlTypeAttributeBase
    {
        public override string ToSqlServerDefinition() => "money";
    }

    public class DateTime2TypeAttribute : SqlTypeAttributeBase
    {
        public override string ToSqlServerDefinition() => "datetime2(7)";
    }

    public class DateTimeOffsetTypeAttribute : SqlTypeAttributeBase
    {
        public int Precision { get; set; }

        public override string ToSqlServerDefinition() => "datetimeoffset";
    }

    public class DecimalTypeAttribute : SqlTypeAttributeBase
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

    public class DoubleTypeAttribute : SqlTypeAttributeBase
    {
        public override string ToSqlServerDefinition() => $"double";
    }

    public class GuidTypeAttribute : SqlTypeAttributeBase
    {
        public override string ToSqlServerDefinition() => $"uniqueidentifier";
    }

    public class FloatTypeAttribute : SqlTypeAttributeBase
    {
        public override string ToSqlServerDefinition() => $"float";
    }

    public class TimeTypeAttribute : SqlTypeAttributeBase
    {
        public override string ToSqlServerDefinition() => $"time";
    }

    public class XmlTypeAttribute : SqlTypeAttributeBase
    {
        public          int    Length                  { get; set; }
        public override string ToSqlServerDefinition() => $"xml";
    }

    public class RowVersionTypeAttribute : SqlTypeAttributeBase
    {
        public override string ToSqlServerDefinition() => $"rowversion";
    }

    public class CustomTypeAttribute : SqlTypeAttributeBase
    {
        public string CustomName { get; set; }

        public CustomTypeAttribute(string customName)
        {
            CustomName = customName;
        }

        public override string ToSqlServerDefinition() => CustomName;
    }
}