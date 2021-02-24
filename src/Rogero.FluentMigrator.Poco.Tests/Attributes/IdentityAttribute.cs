using System;

namespace Rogero.FluentMigrator.Poco.Tests
{
    public class IdentityAttribute : Attribute
    {
        public int Seed      { get; }
        public int Increment { get; }

        public IdentityAttribute(int seed = 1, int increment = 1)
        {
            Seed      = seed;
            Increment = increment;
        }
    }
}