using Xunit.Abstractions;

namespace Rogero.FluentMigrator.Poco.Tests.Infrastructure;

public class UnitTestBaseWithConsoleRedirection
{
    protected ITestOutputHelper _outputHelper;

    public UnitTestBaseWithConsoleRedirection(ITestOutputHelper outputHelperHelper)
    {
        Console.SetOut(new TestOutputHelperToTextWriterAdapter(outputHelperHelper));
        _outputHelper = outputHelperHelper;
    }
}