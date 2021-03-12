using System;
using System.Data;
using Rogero.FluentMigrator.Poco.Attributes;
using Rogero.FluentMigrator.Poco.RelationalTypes;

namespace Rogero.FluentMigrator.Poco.Tests
{
    public class MyDecimalSqlTypeAttribute : DecimalTypeAttribute
    {
        public MyDecimalSqlTypeAttribute() : base(38, 12) { }
    }

    public record Customer
    {
        [PrimaryKey()]
        [Identity()]
        public int Id { get; set; }

        [StringType(100)]
        public string Name { get; set; }
    }

    [TableName("Sales.Order")]
    public record Order2(
        [property: PrimaryKey] [property: Identity]
        int OrderNumber,
        [property: StringType(200)] string  CustomerName,
        [property: StringType(100)] string? PONumber

    )
    {
        [CascadeRule(Rule.None)]
        public int CustomerId { get; set; }
    }

    public record OrderLine2(
        [property: PrimaryKey] [property: ForeignKeyRef(typeof(Order2), Rule.Cascade, nameof(Order2.OrderNumber))]
        int OrderNumber,
        [property: PrimaryKey]
        int OrderLineNumber,
        [property: ForeignKeyRef(typeof(Part2), Rule.Cascade, nameof(Part2.PartNumber))] [property: StringType(50)]
        string PartNumber,
        [property: MyDecimalSqlType]
        decimal Quantity
    );

    public class OrderRelease2
    {
        [PrimaryKey()]
        [ForeignKeyRef(typeof(OrderLine2), Rule.Cascade, nameof(Order2.OrderNumber), foreignKeyGroupId: "fk_OrderLine")]
        public int OrderNumber { get; }

        [PrimaryKey]
        [ForeignKeyRef(typeof(OrderLine2), Rule.Cascade, "OrderLineNumber", "fk_OrderLine")]
        public int OrderLineNumber { get; set; }

        [PrimaryKey]
        public int OrderReleaseNumber { get; set; }

        public DateTime ShipDate { get; set; }

        [MyDecimalSqlType]
        public decimal Quantity { get; set; }
    }

    [TableName("Inventory.Part2")]
    public class Part2
    {
        [PrimaryKey()]
        [StringType(50)]
        public string PartNumber { get; set; }

        [StringType(Length = 50)]
        public string PartDescription { get; set; }

        [RowVersionType]
        public byte[] RowVersion { get; set; }
    }
}