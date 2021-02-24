namespace Rogero.FluentMigrator.Poco
{
    public class ColumnDataIdentity
    {
        public int Seed      { get; set; }
        public int Increment { get; set; }
        
        public ColumnDataIdentity(int seed, int increment)
        {
            Seed      = seed;
            Increment = increment;
        }

        public void Deconstruct(out int seed, out int increment)
        {
            seed      = Seed;
            increment = Increment;
        }
    }
}