using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Rogero.FluentMigrator.Poco.Tests
{
    public class TableDataFactoryTests : UnitTestBaseWithConsoleRedirection
    {
        [Fact]
        public void PrintDataToConsole()
        {
            var types = new List<Type>()
            {
                typeof(Order2), 
                typeof(OrderLine2),
                typeof(OrderRelease2), 
                typeof(Part2)
            };
            
            var tableDatas = types
                .Select(TableDataFactory.CreateTableDataFromType)
                .ToList();

            tableDatas.ForEach(tableData => Console.WriteLine(tableData.ToString()));
        }

        public TableDataFactoryTests(ITestOutputHelper outputHelperHelper) : base(outputHelperHelper) { }
    }
}