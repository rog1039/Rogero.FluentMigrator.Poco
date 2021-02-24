using System;
using System.Collections.Generic;
using System.Linq;
using FluentMigrator;

namespace Rogero.FluentMigrator.Poco.Tests
{
    [Migration(1)]
    [Tags("Group1")]
    public class Migration1 : Migration
    {
        public override void Up()
        {
            var types   = new List<Type>() {typeof(Part2), typeof(Order2), typeof(OrderLine2), typeof(OrderRelease2)};
            var configs = types.Select(TableDataFactory.CreateTableDataFromType);
            foreach (var creationData in configs)
            {
                this.Apply(creationData);
            }
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}