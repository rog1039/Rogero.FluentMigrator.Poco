using FluentMigrator;
using FluentMigrator.SqlServer;

namespace Rogero.FluentMigrator.Poco.Tests.Runner
{
    [Migration(2)]
    [Tags("Group2")]
    public class Migration2OutOfOrder: Migration
    {
        public override void Up()
        {
            /*
             * This will fall because order line depends on the order table being created first.
             * This is a migration to prove that is true.
             */
            
            Create.Table("OrderLine")
                .WithColumn("OrderNumber").AsInt32().PrimaryKey().NotNullable()
                .ForeignKey("Order", "OrderNumber");
            ;
            Create.Table("Order")
                .WithColumn("OrderNumber").AsInt32().PrimaryKey().NotNullable().Identity(1, 1)
                ;
        }

        public override void Down()
        {
            throw new System.NotImplementedException();
        }
    }
}