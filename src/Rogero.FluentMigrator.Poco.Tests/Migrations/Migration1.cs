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
        public List<Type> Types { get; } = new()
        {
            typeof(Order2),
            typeof(Customer2),
            typeof(OrderLine2),
            typeof(OrderRelease2),
            typeof(Part2),
        };
        
        public override void Up()
        {
            var dbModel = DbModelFactory.GenerateModel(Types);
            this.Apply(dbModel);
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}