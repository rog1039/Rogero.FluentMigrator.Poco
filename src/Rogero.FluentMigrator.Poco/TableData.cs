using System.Text;

namespace Rogero.FluentMigrator.Poco;

public class TableData
{
    public Type             SourceType { get; set; }
    public SchemaTableNames TableName  { get; set; }

    public List<ColumnData> ColumnCreationData { get; set; } = new();

    public List<ColumnDataForeignKey> ForeignKeyParts  { get; } = new();
    public List<MultiForeignKeyData>  MultiForeignKeys { get; } = new();

    public void Deconstruct(out SchemaTableNames table, out IList<ColumnData> columns)
    {
        table   = TableName;
        columns = ColumnCreationData;
    }

    public void FinishForeignKeys()
    {
        FillOutForeignPartsOfForeignKeys();
        BuildMultiForeignKeys();
    }

    /// <summary>
    /// This basically fills out the foreign schema/table names in each foreign key ref.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private void FillOutForeignPartsOfForeignKeys()
    {
        throw new NotImplementedException();
    }

    public void BuildMultiForeignKeys()
    {
        var foreignKeyParts = ColumnCreationData
            .Where(z => z.ForeignKeyInformation?.IsMultiKey == true)
            .Select(z => z.ForeignKeyInformation!)
            .ToList();
        var multiForeignKeys = MultiForeignKeyData.CreateFromForeignKeyDatas(foreignKeyParts);
        MultiForeignKeys.Clear();
        MultiForeignKeys.AddRange(multiForeignKeys);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Table: {TableName}");
        sb.AppendLine(ColumnCreationData.ToStringTable());
        // foreach (var column in ColumnCreationData)
        // {
        //     sb.Append("   ");
        //     sb.AppendLine(column.ToString());
        // }

        foreach (var multiForeignKey in MultiForeignKeys)
        {
            var foreignColumnNames = multiForeignKey.ForeignColumnNames.StringJoin(",");
            var primaryColumnNames = multiForeignKey.PrimaryColumnNames.StringJoin(",");
            sb.Append("   ");
            sb.AppendLine($"FK: [{multiForeignKey.GroupName}] ({foreignColumnNames}) -> " +
                          $"{multiForeignKey.PrimarySchemaName}.{multiForeignKey.PrimaryTableName}.({primaryColumnNames}) [{multiForeignKey.GetForeignKeyName()}]");
        }
        return sb.ToString();
    }
}